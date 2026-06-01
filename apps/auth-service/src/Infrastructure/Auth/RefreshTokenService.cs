using System.Security.Cryptography;
using DeskMatch.AuthService.Application.Auth;
using DeskMatch.AuthService.Infrastructure.Identity;
using DeskMatch.AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DeskMatch.AuthService.Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(AuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RefreshToken> GenerateAsync(ApplicationUser user)
    {
        var expiresAt = DateTime.UtcNow.AddDays(GetExpirationDays());

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        user.RefreshTokenExpiryTime = expiresAt;

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        return token;
    }

    public Task<RefreshToken?> FindValidAsync(string token)
        => _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == token &&
                !rt.IsUsed &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow);

    public async Task MarkUsedAsync(RefreshToken token)
    {
        token.IsUsed = true;
        await _context.SaveChangesAsync();
    }

    private int GetExpirationDays()
    {
        var raw = _configuration["JWT_REFRESH_TOKEN_EXPIRATION_DAYS"]
                  ?? _configuration["Jwt:RefreshTokenExpirationDays"];
        return int.TryParse(raw, out var days) && days > 0 ? days : 7;
    }
}
