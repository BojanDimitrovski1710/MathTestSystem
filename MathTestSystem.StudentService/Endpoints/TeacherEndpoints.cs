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

        group.MapGet("/{teacherId}/students", GetTeacherStudents)
            .WithName("GetTeacherStudents")
            .WithSummary("Returns all students and their exam summaries for the given teacher.")
            .Produces<TeacherStudentsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetTeacherStudents(
        string teacherId,
        ITeacherRepository teacherRepo,
        IStudentRepository studentRepo,
        IExamRepository examRepo)
    {
        Teacher? teacher = await teacherRepo.GetByTeacherIdAsync(teacherId);

        if (teacher is null)
            return Results.NotFound(ResultCodes.TeacherNotFound);

        IEnumerable<Student> students = await studentRepo.GetByTeacherUidAsync(teacher.Uid);

        List<StudentOverviewResponse> overviews = [];

        foreach (Student student in students)
        {
            IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);

            overviews.Add(new StudentOverviewResponse(
                student.Uid,
                student.StudentId,
                exams.Select(e => new ExamSummaryResponse(
                    e.Uid,
                    e.ExamId,
                    e.SubmittedAt,
                    e.Score,
                    e.Tasks.Count,
                    e.Tasks.Count(t => t.IsCorrect)))
                .ToList()));
        }

        return Results.Ok(new TeacherStudentsResponse(teacherId, overviews));
    }
}
