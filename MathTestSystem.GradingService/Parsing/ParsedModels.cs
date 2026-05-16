namespace MathTestSystem.GradingService.Parsing;

public record ParsedTeacherExam(string TeacherId, IReadOnlyList<ParsedStudent> Students);

public record ParsedStudent(string StudentId, IReadOnlyList<ParsedExam> Exams);

public record ParsedExam(string ExamId, IReadOnlyList<ParsedTask> Tasks);

public record ParsedTask(string TaskId, string Expression, decimal StudentAnswer);
