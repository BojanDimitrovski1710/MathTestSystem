namespace MathTestSystem.Infrastructure.Auth;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string username, string role);
}
