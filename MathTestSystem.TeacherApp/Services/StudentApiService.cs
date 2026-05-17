using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MathTestSystem.TeacherApp.Models;
using MathTestSystem.TeacherApp.State;

namespace MathTestSystem.TeacherApp.Services;

public class StudentApiService : IStudentApiService
{
    private readonly HttpClient _http = new();
    private readonly AuthState _authState;
    private const string BaseUrl = "http://localhost:5000/api/teachers";

    public StudentApiService(AuthState authState)
    {
        _authState = authState;
    }

    public async Task<List<StudentSummaryResponse>> GetTeacherStudentsAsync(string teacherId)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{BaseUrl}/{teacherId}/students");

        if (_authState.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

        HttpResponseMessage response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Gateway returned {(int)response.StatusCode}: {error}");
        }

        List<StudentSummaryResponse>? result = await response.Content.ReadFromJsonAsync<List<StudentSummaryResponse>>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new InvalidOperationException("Gateway returned an empty response.");
    }
}
