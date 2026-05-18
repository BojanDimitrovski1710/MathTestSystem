namespace MathTestSystem.StudentApp.Services;

public interface IAuthApiService
{
    Task<(string Token, string Role)> LoginAsync(string username, string password);
}
