using MathTestSystem.TeacherApp.Models;

namespace MathTestSystem.TeacherApp.Services;

public interface IStudentApiService
{
    Task<TeacherStudentsResponse> GetTeacherStudentsAsync(string teacherId);
}
