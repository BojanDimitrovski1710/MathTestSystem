namespace MathTestSystem.TeacherApp.Services;

public interface IAuthApiService
{
    Task<string> LoginAsync(string username, string password);
}
