using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Workspaces;

public class Workspace : AggregateRoot<Guid>
{
    public Workspace(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private Workspace() { } // EF Core

    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Capacity { get; set; } = 1;
    public decimal PricePerHour { get; set; } = 0;
    public decimal? PricePerDay { get; set; }
    public decimal? PricePerMonth { get; set; }
    public List<string>? Amenities { get; set; }
    public List<string>? Images { get; set; }
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; } = true;

    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}