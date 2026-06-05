using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Companies;

public class Company : AggregateRoot<Guid>
{
    public Company(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private Company() { } // EF Core

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public Guid? OwnerId { get; set; }
    public bool IsActive { get; set; } = true;
}