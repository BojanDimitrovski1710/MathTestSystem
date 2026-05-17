using MathTestSystem.TeacherApp.Models;

namespace MathTestSystem.TeacherApp.Services;

public interface IGradingApiService
{
    Task<GradeExamResponse> GradeAsync(string xmlContent);
}
