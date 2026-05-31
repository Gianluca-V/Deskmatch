using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeskMatch.AuthService.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DeskMatch.AuthService.Application.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse CreateToken(ApplicationUser user, string role)
    {
        var secret = GetRequiredSetting("JWT_SECRET", "Jwt:Secret");
        var issuer = GetRequiredSetting("JWT_ISSUER", "Jwt:Issuer");
        var audience = GetRequiredSetting("JWT_AUDIENCE", "Jwt:Audience");
        var expiresMinutes = GetExpiresMinutes();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var userResponse = new UserResponse(user.Id, user.Name, user.Email ?? string.Empty, role);

        return new LoginResponse(accessToken, expiresAt, userResponse);
    }

    private string GetRequiredSetting(string environmentKey, string configurationKey)
    {
        var value = FirstConfiguredValue(environmentKey, configurationKey);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{environmentKey} is required.");
        }

        return value;
    }

    private int GetExpiresMinutes()
    {
        var rawValue = FirstConfiguredValue("JWT_EXPIRES_MINUTES", "Jwt:AccessTokenExpirationMinutes");

        return int.TryParse(rawValue, out var value) && value > 0 ? value : 60;
    }

    private string? FirstConfiguredValue(params string[] keys)
    {
        return keys
            .Select(key => _configuration[key])
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
