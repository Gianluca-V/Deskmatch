using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Testimonials;

public class Testimonial : AggregateRoot<Guid>
{
    public Testimonial(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private Testimonial() { } // EF Core

    public string AuthorName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
