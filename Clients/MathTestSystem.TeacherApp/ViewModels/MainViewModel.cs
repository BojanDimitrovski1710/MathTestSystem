using MathTestSystem.TeacherApp.Models;

namespace MathTestSystem.TeacherApp.ViewModels;

public class StudentResultViewModel(StudentGradeResult model)
{
    public string StudentId => model.StudentId;
    public Guid StudentUid => model.StudentUid;
    public IReadOnlyList<ExamResultViewModel> Exams { get; } =
        model.Exams.Select(e => new ExamResultViewModel(e)).ToList();
}

public class ExamResultViewModel(ExamGradeResult model)
{
    public string ExamId => model.ExamId;
    public decimal Score => model.Score;
    public IReadOnlyList<TaskResultViewModel> Tasks { get; } =
        model.Tasks.Select(t => new TaskResultViewModel(t)).ToList();
}

public class TaskResultViewModel(TaskGradeResult model)
{
    public string Expression => model.Expression;
    public decimal StudentAnswer => model.StudentAnswer;
    public decimal? CorrectAnswer => model.CorrectAnswer;
    public bool IsCorrect => model.IsCorrect;

    public string StatusIcon => model.HasError ? "⚠" : model.IsCorrect ? "✓" : "✗";
    public string StatusColor => model.HasError ? "Orange" : model.IsCorrect ? "Green" : "Red";

    public string CorrectAnswerText => model.HasError
        ? $"[{model.ErrorCode}]"
        : model.IsCorrect
            ? string.Empty
            : $"(correct: {model.CorrectAnswer})";
}
