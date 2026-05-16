using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Models;

namespace MathTestSystem.StudentService.Endpoints;

public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/students")
            .WithTags("Students");

        group.MapGet("/{studentUid:guid}/exams", GetExams)
            .WithName("GetStudentExams")
            .WithSummary("Returns a summary of all exams for the given student.")
            .Produces<IReadOnlyList<ExamSummaryResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{studentUid:guid}/exams/{examUid:guid}", GetExamDetail)
            .WithName("GetStudentExamDetail")
            .WithSummary("Returns the full detail of a single exam, including all tasks.")
            .Produces<ExamDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetExams(
        Guid studentUid,
        IStudentRepository studentRepo,
        IExamRepository examRepo)
    {
        Student? student = await studentRepo.GetByUidAsync(studentUid);

        if (student is null)
            return Results.NotFound(ResultCodes.StudentNotFound);

        IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);

        IReadOnlyList<ExamSummaryResponse> response = exams
            .Select(e => new ExamSummaryResponse(
                e.Uid,
                e.ExamId,
                e.SubmittedAt,
                e.Score,
                e.Tasks.Count,
                e.Tasks.Count(t => t.IsCorrect)))
            .ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetExamDetail(
        Guid studentUid,
        Guid examUid,
        IStudentRepository studentRepo,
        IExamRepository examRepo)
    {
        Student? student = await studentRepo.GetByUidAsync(studentUid);

        if (student is null)
            return Results.NotFound(ResultCodes.StudentNotFound);

        Exam? exam = await examRepo.GetWithTasksAsync(examUid);

        if (exam is null || exam.Student.Uid != student.Uid)
            return Results.NotFound(ResultCodes.ExamNotFound);

        ExamDetailResponse response = new(
            exam.Uid,
            exam.ExamId,
            exam.SubmittedAt,
            exam.Score,
            exam.Tasks
                .Select(t => new TaskResponse(
                    t.TaskId,
                    t.Expression,
                    t.StudentAnswer,
                    t.CorrectAnswer,
                    t.IsCorrect,
                    t.ErrorMessage))
                .ToList());

        return Results.Ok(response);
    }
}
