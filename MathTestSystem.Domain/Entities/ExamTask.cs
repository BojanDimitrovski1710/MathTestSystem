namespace MathTestSystem.Domain.Entities;

public class ExamTask
{
    public int Id { get; set; }

    /// <summary>
    /// The task identifier as it appears in the uploaded XML (e.g. "1").
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    public Guid ExamUid { get; set; }
    public Exam Exam { get; set; } = null!;

    /// <summary>
    /// The raw expression from the XML, left of the "=" sign (e.g. "2+3/6-4").
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// The answer the student provided, right of the "=" sign.
    /// </summary>
    public decimal StudentAnswer { get; set; }

    /// <summary>
    /// The computed correct answer. Null if the expression could not be evaluated.
    /// </summary>
    public decimal? CorrectAnswer { get; set; }

    public bool IsCorrect { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// True if the expression was malformed and could not be evaluated.
    /// Derived from <see cref="ErrorMessage"/> — no separate column needed.
    /// </summary>
    public bool HasError => ErrorMessage is not null;
}
