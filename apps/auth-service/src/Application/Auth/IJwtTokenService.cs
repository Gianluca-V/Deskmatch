using DeskMatch.AuthService.Infrastructure.Identity;

namespace DeskMatch.AuthService.Application.Auth;

public interface IJwtTokenService
{
    LoginResponse CreateToken(ApplicationUser user, string role);
}
