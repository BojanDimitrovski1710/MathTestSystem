using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Models;

namespace MathTestSystem.StudentService.Endpoints;

public static class TeacherEndpoints
{
    public static void MapTeacherEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/teachers")
            .WithTags("Teachers");

        group.MapGet("/{teacherId}/students", GetStudents)
            .WithName("GetTeacherStudents")
            .WithSummary("Returns all students and their exam summaries for the given teacher.")
            .Produces<IReadOnlyList<StudentSummaryResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetStudents(
        string teacherId,
        ITeacherRepository teacherRepo,
        IStudentRepository studentRepo,
        IExamRepository examRepo)
    {
        Teacher? teacher = await teacherRepo.GetByTeacherIdAsync(teacherId);

        if (teacher is null)
            return Results.NotFound(ResultCodes.TeacherNotFound);

        IEnumerable<Student> students = await studentRepo.GetByTeacherUidAsync(teacher.Uid);

        List<StudentSummaryResponse> response = [];

        foreach (Student student in students)
        {
            IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);

            IReadOnlyList<ExamSummaryResponse> examSummaries = exams
                .Select(e => new ExamSummaryResponse(
                    e.Uid,
                    e.ExamId,
                    e.SubmittedAt,
                    e.Score,
                    e.Tasks.Count,
                    e.Tasks.Count(t => t.IsCorrect)))
                .ToList();

            response.Add(new StudentSummaryResponse(student.Uid, student.StudentId, examSummaries));
        }

        return Results.Ok(response);
    }
}
