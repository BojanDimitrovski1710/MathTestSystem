using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using MathTestSystem.TeacherApp.Models;
using MathTestSystem.TeacherApp.State;

namespace MathTestSystem.TeacherApp.Services;

public class GradingApiService : IGradingApiService
{
    private readonly HttpClient _http;
    private readonly AuthState _authState;
    private const string GradeEndpoint = "http://localhost:5000/api/exams/grade";

    public GradingApiService(AuthState authState)
    {
        _http = new HttpClient();
        _authState = authState;
    }

    public async Task<GradeExamResponse> GradeAsync(string xmlContent)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, GradeEndpoint)
        {
            Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml")
        };

        if (_authState.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

        HttpResponseMessage response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Gateway returned {(int)response.StatusCode}: {error}");
        }

        GradeExamResponse? result = await response.Content.ReadFromJsonAsync<GradeExamResponse>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new InvalidOperationException("Gateway returned an empty response.");
    }
}
