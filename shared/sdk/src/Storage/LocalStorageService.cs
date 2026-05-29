namespace DeskMatch.SDK.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _basePath;

    public LocalStorageService()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "DeskMatch", "Storage");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(string container, string fileName, Stream content, string contentType)
    {
        var containerPath = Path.Combine(_basePath, container);
        Directory.CreateDirectory(containerPath);

        var filePath = Path.Combine(containerPath, fileName);
        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);

        return filePath;
    }

    public Task<Stream> DownloadAsync(string container, string fileName)
    {
        var filePath = Path.Combine(_basePath, container, fileName);
        var stream = File.OpenRead(filePath);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string container, string fileName)
    {
        var filePath = Path.Combine(_basePath, container, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string container, string fileName)
    {
        var filePath = Path.Combine(_basePath, container, fileName);
        return Task.FromResult(filePath);
    }
}