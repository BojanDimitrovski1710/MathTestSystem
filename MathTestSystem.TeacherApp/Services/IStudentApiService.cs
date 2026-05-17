using MathTestSystem.TeacherApp.Models;

namespace MathTestSystem.TeacherApp.Services;

public interface IStudentApiService
{
    Task<List<StudentSummaryResponse>> GetTeacherStudentsAsync(string teacherId);
}
