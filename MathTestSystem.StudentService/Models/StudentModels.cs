namespace MathTestSystem.StudentService.Models;

public record ExamSummaryResponse(
    Guid ExamUid,
    string ExamId,
    DateTime SubmittedAt,
    decimal Score,
    int TotalTasks,
    int CorrectTasks);

public record ExamDetailResponse(
    Guid ExamUid,
    string ExamId,
    DateTime SubmittedAt,
    decimal Score,
    IReadOnlyList<TaskResponse> Tasks);

/// <param name="ErrorCode">Populated when the expression could not be evaluated. See ResultCodes.</param>
public record TaskResponse(
    string TaskId,
    string Expression,
    decimal StudentAnswer,
    decimal? CorrectAnswer,
    bool IsCorrect,
    string? ErrorCode)
{
    public bool HasError => ErrorCode is not null;
}

public record TeacherStudentsResponse(
    string TeacherId,
    IReadOnlyList<StudentOverviewResponse> Students);

public record StudentOverviewResponse(
    Guid StudentUid,
    string StudentId,
    IReadOnlyList<ExamSummaryResponse> Exams);
