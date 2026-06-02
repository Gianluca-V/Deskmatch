using DeskMatch.AuthService.Infrastructure.Identity;

namespace DeskMatch.AuthService.Application.Auth;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateAsync(ApplicationUser user);
    Task<RefreshToken?> FindValidAsync(string token);
    Task MarkUsedAsync(RefreshToken token);
}
