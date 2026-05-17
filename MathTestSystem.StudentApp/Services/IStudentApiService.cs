using MathTestSystem.StudentApp.Models;

namespace MathTestSystem.StudentApp.Services;

public interface IStudentApiService
{
    Task<StudentDashboardResponse> GetDashboardAsync(string studentId);
}
