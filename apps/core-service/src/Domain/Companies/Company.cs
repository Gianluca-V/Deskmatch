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
    public string? ContactEmail { get; set; }
    public bool IsVerified { get; set; } = false;
    public Guid? OwnerId { get; set; }
    public bool IsActive { get; set; } = true;

    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}