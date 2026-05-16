using System.Net.Http;
using System.Net.Http.Json;
using MathTestSystem.TeacherApp.Models;

namespace MathTestSystem.TeacherApp.Services;

public class StudentApiService : IStudentApiService
{
    private readonly HttpClient _http = new();
    private const string BaseUrl = "http://localhost:5002/api/teachers";

    public async Task<TeacherStudentsResponse> GetTeacherStudentsAsync(string teacherId)
    {
        HttpResponseMessage response = await _http.GetAsync($"{BaseUrl}/{teacherId}/students");

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Student service returned {(int)response.StatusCode}: {error}");
        }

        TeacherStudentsResponse? result = await response.Content.ReadFromJsonAsync<TeacherStudentsResponse>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new InvalidOperationException("Student service returned an empty response.");
    }
}
