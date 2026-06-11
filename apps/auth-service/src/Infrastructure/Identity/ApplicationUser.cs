using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DeskMatch.AuthService.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? FirstName { get; set; }

    [MaxLength(128)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}
