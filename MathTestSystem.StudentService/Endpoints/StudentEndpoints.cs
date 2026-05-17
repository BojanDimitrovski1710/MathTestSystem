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

        group.MapGet("/{studentId}/dashboard", GetDashboard)
            .WithName("GetStudentDashboard")
            .WithSummary("Returns overall stats and a per-teacher exam breakdown for the given student.")
            .Produces<StudentDashboardResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/{studentUid:guid}/exams", GetExams)
            .WithName("GetStudentExams")
            .WithSummary("Returns a summary of all exams for the given student.")
            .Produces<IReadOnlyList<ExamSummaryResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/{studentUid:guid}/exams/{examUid:guid}", GetExamDetail)
            .WithName("GetStudentExamDetail")
            .WithSummary("Returns the full detail of a single exam, including all tasks.")
            .Produces<ExamDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetDashboard(
        string studentId,
        IStudentRepository studentRepo,
        IExamRepository examRepo)
    {
        Student? student = await studentRepo.GetByStudentIdAsync(studentId);

        if (student is null)
            return Results.NotFound(ResultCodes.StudentNotFound);

        IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);
        List<Exam> examList = exams.ToList();

        int overallCorrect = examList.Sum(e => e.Tasks.Count(t => t.IsCorrect));
        int overallTotal = examList.Sum(e => e.Tasks.Count);
        decimal overallScore = overallTotal > 0
            ? Math.Round((decimal)overallCorrect / overallTotal * 100, 2)
            : 0m;

        // In the current domain a student belongs to one teacher.
        // The grouping structure supports multiple teachers if that changes.
        TeacherDashboardEntry teacherEntry = new(
            student.Teacher.TeacherId,
            overallCorrect,
            overallTotal,
            overallScore,
            examList.Select(e =>
            {
                int correct = e.Tasks.Count(t => t.IsCorrect);
                int total = e.Tasks.Count;
                decimal score = total > 0
                    ? Math.Round((decimal)correct / total * 100, 2)
                    : 0m;

                return new ExamDashboardEntry(
                    e.Uid,
                    e.ExamId,
                    e.SubmittedAt,
                    score,
                    correct,
                    total,
                    e.Tasks
                        .Select(t => new TaskResponse(
                            t.TaskId,
                            t.Expression,
                            t.StudentAnswer,
                            t.CorrectAnswer,
                            t.IsCorrect,
                            t.ErrorMessage))
                        .ToList());
            }).ToList());

        StudentDashboardResponse response = new(
            student.StudentId,
            overallCorrect,
            overallTotal,
            overallScore,
            [teacherEntry]);

        return Results.Ok(response);
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
