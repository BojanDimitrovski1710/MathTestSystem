namespace MathTestSystem.TeacherApp.Models;

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

public record TaskGradeResult(
    string TaskId,
    string Expression,
    decimal StudentAnswer,
    decimal? CorrectAnswer,
    bool IsCorrect,
    bool HasError,
    string? ErrorCode);
