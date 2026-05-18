namespace MathTestSystem.StudentApp.Models;

public record StudentDashboardResponse(
    string StudentId,
    int OverallCorrect,
    int OverallTotal,
    decimal OverallScore,
    IReadOnlyList<TeacherDashboardEntry> Teachers);

public record TeacherDashboardEntry(
    string TeacherId,
    int CorrectTasks,
    int TotalTasks,
    decimal Score,
    IReadOnlyList<ExamDashboardEntry> Exams);

public record ExamDashboardEntry(
    Guid ExamUid,
    string ExamId,
    DateTime SubmittedAt,
    decimal Score,
    int CorrectTasks,
    int TotalTasks,
    IReadOnlyList<TaskEntry> Tasks);

public record TaskEntry(
    string TaskId,
    string Expression,
    decimal StudentAnswer,
    decimal? CorrectAnswer,
    bool IsCorrect,
    string? ErrorCode)
{
    public bool HasError => ErrorCode is not null;
}
