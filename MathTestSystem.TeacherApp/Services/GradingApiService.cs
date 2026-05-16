using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using MathTestSystem.TeacherApp.Models;

namespace MathTestSystem.TeacherApp.Services;

public class GradingApiService : IGradingApiService
{
    private readonly HttpClient _http;
    private const string GradeEndpoint = "http://localhost:5001/api/exams/grade";

    public GradingApiService()
    {
        _http = new HttpClient();
    }

    public async Task<GradeExamResponse> GradeAsync(string xmlContent)
    {
        StringContent content = new(xmlContent, Encoding.UTF8, "application/xml");
        HttpResponseMessage response = await _http.PostAsync(GradeEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Grading service returned {(int)response.StatusCode}: {error}");
        }

        GradeExamResponse? result = await response.Content.ReadFromJsonAsync<GradeExamResponse>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new InvalidOperationException("Grading service returned an empty response.");
    }
}
