using DeskMatch.AuthService.Application.Auth;
using DeskMatch.SDK.Redis;

namespace DeskMatch.AuthService.Infrastructure.Auth;

public sealed class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ICacheService _cache;
    private const string KeyPrefix = "auth:blacklist:";

    public TokenBlacklistService(ICacheService cache)
    {
        _cache = cache;
    }

    public Task BlacklistAsync(string jti, TimeSpan expiresIn)
        => _cache.SetAsync($"{KeyPrefix}{jti}", true, expiresIn);

    public Task<bool> IsBlacklistedAsync(string jti)
        => _cache.ExistsAsync($"{KeyPrefix}{jti}");
}
