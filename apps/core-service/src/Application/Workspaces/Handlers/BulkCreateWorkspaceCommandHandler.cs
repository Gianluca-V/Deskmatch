using System.Collections.Concurrent;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Models;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Geocoding;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.OpenSearch.Documents;
using DeskMatch.SDK.Storage;
using Microsoft.Extensions.Logging;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class BulkCreateWorkspaceCommandHandler : ICommandHandler<BulkConfirmCommand, BulkCreateResponse>
{
    private static readonly ConcurrentDictionary<Guid, DateTime> _lastUploadPerCompany = new();

    private readonly IWorkspaceRepository _repository;
    private readonly IGeocodingService _geocoding;
    private readonly IStorageService _storage;
    private readonly IOpenSearchRepository<WorkspaceDocument> _searchRepo;
    private readonly IOllamaClient _ollama;
    private readonly ILogger<BulkCreateWorkspaceCommandHandler> _logger;

    public BulkCreateWorkspaceCommandHandler(
        IWorkspaceRepository repository,
        IGeocodingService geocoding,
        IStorageService storage,
        IOpenSearchRepository<WorkspaceDocument> searchRepo,
        IOllamaClient ollama,
        ILogger<BulkCreateWorkspaceCommandHandler> logger)
    {
        _repository = repository;
        _geocoding = geocoding;
        _storage = storage;
        _searchRepo = searchRepo;
        _ollama = ollama;
        _logger = logger;
    }

    public async Task<BulkCreateResponse> HandleAsync(
        BulkConfirmCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_lastUploadPerCompany.TryGetValue(command.CompanyId, out var lastUpload))
        {
            if (DateTime.UtcNow - lastUpload < TimeSpan.FromSeconds(30))
            {
                var remaining = (int)(30 - (DateTime.UtcNow - lastUpload).TotalSeconds);
                return new BulkCreateResponse(0, 0, [new BulkError(0, "", $"Debés esperar {remaining} segundos antes de otra carga masiva")]);
            }
        }

        if (command.Offices.Count > 500)
        {
            return new BulkCreateResponse(0, command.Offices.Count, [new BulkError(0, "", "Máximo 500 oficinas por carga")]);
        }

        var imageDict = BuildImageDictionary(command.Images);
        var errors = new List<BulkError>();
        var workspaces = new List<Workspace>();
        var documents = new List<WorkspaceDocument>();

        foreach (var office in command.Offices)
        {
            var id = Guid.NewGuid();
            double? lat = office.Latitude;
            double? lng = office.Longitude;

            if (!lat.HasValue && !lng.HasValue &&
                !string.IsNullOrWhiteSpace(office.Address) &&
                !string.IsNullOrWhiteSpace(office.City) &&
                !string.IsNullOrWhiteSpace(office.Country))
            {
                try
                {
                    var query = $"{office.Address}, {office.City}, {office.Country}";
                    var geoResults = await _geocoding.GeocodeAsync(query, cancellationToken);
                    if (geoResults.Length > 0)
                    {
                        lat = geoResults[0].Latitude;
                        lng = geoResults[0].Longitude;
                    }
                }
                catch { }
            }

            var images = new List<string>();
            if (office.ImageFileNames is not null)
            {
                foreach (var imageName in office.ImageFileNames)
                {
                    var key = Path.GetFileNameWithoutExtension(imageName).ToLowerInvariant();
                    if (imageDict.TryGetValue(key, out var matchedFile))
                    {
                        try
                        {
                            var ext = Path.GetExtension(matchedFile.FileName);
                            var storageFileName = $"{id:N}_{key}{ext}";
                            var contentType = GetImageContentType(ext);
                            using var imgStream = new MemoryStream(matchedFile.Content);
                            var url = await _storage.UploadAsync("workspaces", storageFileName, imgStream, contentType);
                            images.Add(url);
                        }
                        catch
                        {
                            errors.Add(new BulkError(office.TempId, office.Name, $"No se pudo subir la imagen '{imageName}'"));
                        }
                    }
                    else
                    {
                        errors.Add(new BulkError(office.TempId, office.Name, $"No se encontró la imagen '{imageName}' entre los archivos subidos"));
                    }
                }
            }

            var workspace = new Workspace(id)
            {
                CompanyId = command.CompanyId,
                Name = office.Name,
                Description = office.Description,
                Address = office.Address,
                City = office.City,
                Country = office.Country,
                Latitude = lat,
                Longitude = lng,
                Capacity = office.Capacity,
                PricePerHour = office.PricePerHour,
                PricePerDay = office.PricePerDay,
                PricePerMonth = office.PricePerMonth,
                Amenities = office.Amenities,
                Images = images.Count > 0 ? images : null,
                IsActive = true,
            };

            workspaces.Add(workspace);
            documents.Add(ToDocument(workspace));
        }

        foreach (var w in workspaces)
        {
            await _repository.AddAsync(w, cancellationToken);
        }

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar workspaces en bulk. Inner: {Inner}",
                ex.InnerException?.Message ?? "(sin inner)");
            throw;
        }

        try
        {
            if (_ollama.IsAvailable)
            {
                foreach (var doc in documents)
                {
                    doc.NameVector = await _ollama.GetEmbeddingAsync(doc.Name);
                    doc.DescriptionVector = await _ollama.GetEmbeddingAsync(doc.Description ?? "");
                }
            }

            await _searchRepo.BulkIndexAsync(documents, index: "offices");
        }
        catch { }

        _lastUploadPerCompany[command.CompanyId] = DateTime.UtcNow;
        CleanupOldEntries();

        return new BulkCreateResponse(
            workspaces.Count,
            command.Offices.Count,
            errors
        );
    }

    private static Dictionary<string, BulkImageFile> BuildImageDictionary(IReadOnlyList<BulkImageFile> images)
    {
        var dict = new Dictionary<string, BulkImageFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var img in images)
        {
            var key = Path.GetFileNameWithoutExtension(img.FileName).ToLowerInvariant();
            dict.TryAdd(key, img);
        }

        return dict;
    }

    private static void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-5);
        foreach (var kvp in _lastUploadPerCompany)
        {
            if (kvp.Value < cutoff)
                _lastUploadPerCompany.TryRemove(kvp.Key, out _);
        }
    }

    private static string GetImageContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    private static WorkspaceDocument ToDocument(Workspace w) => new()
    {
        Id = w.Id.ToString(),
        Name = w.Name,
        Description = w.Description,
        City = w.City,
        Country = w.Country,
        Address = w.Address,
        Capacity = w.Capacity,
        PricePerHour = (double)w.PricePerHour,
        Amenities = w.Amenities,
        Images = w.Images,
        Location = w.Latitude.HasValue && w.Longitude.HasValue
            ? new OpenSearch.Client.GeoLocation(w.Latitude.Value, w.Longitude.Value)
            : null,
        Rating = w.Rating,
        ReviewCount = w.ReviewCount,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        DynamicAttributes = w.DynamicAttributes?
            .ToDictionary(a => a.Key, a => (object)(a.Value ?? ""))
    };
}
