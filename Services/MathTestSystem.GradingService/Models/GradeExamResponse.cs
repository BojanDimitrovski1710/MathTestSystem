namespace MathTestSystem.GradingService.Models;

public record GradeExamResponse(
    string TeacherId,
    IReadOnlyList<StudentGradeResult> Students);

public record StudentGradeResult(
    Guid StudentUid,
    string StudentId,
    IReadOnlyList<ExamGradeResult> Exams);

public record ExamGradeResult(
    Guid ExamUid,
    string ExamId,
    decimal Score,
    IReadOnlyList<TaskGradeResult> Tasks);

/// <param name="ErrorCode">Populated when the expression could not be evaluated. See ResultCodes.</param>
public record TaskGradeResult(
    string TaskId,
    string Expression,
    decimal StudentAnswer,
    decimal? CorrectAnswer,
    bool IsCorrect,
    string? ErrorCode)
{
    public bool HasError => ErrorCode is not null;
}
