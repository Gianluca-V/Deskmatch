namespace DeskMatch.AuthService.Application.Auth;

public interface ITokenBlacklistService
{
    Task BlacklistAsync(string jti, TimeSpan expiresIn);
    Task<bool> IsBlacklistedAsync(string jti);
}
