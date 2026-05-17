using System.Net.Http;
using System.Net.Http.Json;

namespace MathTestSystem.TeacherApp.Services;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _http = new();
    private const string LoginEndpoint = "http://localhost:5000/api/auth/login";

    public async Task<string> LoginAsync(string username, string password)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync(LoginEndpoint, new { username, password });

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed ({(int)response.StatusCode}): {error}");
        }

        TokenResponse? result = await response.Content.ReadFromJsonAsync<TokenResponse>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Token ?? throw new InvalidOperationException("Gateway returned an empty token.");
    }

    private record TokenResponse(string Token);
}
