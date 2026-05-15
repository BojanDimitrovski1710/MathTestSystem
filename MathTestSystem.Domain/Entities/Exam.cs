namespace MathTestSystem.Domain.Entities;

public class Exam
{
    public int Id { get; set; }

    /// <summary>
    /// Publicly exposed identifier used in API routes.
    /// Generated on creation — prevents sequential ID enumeration attacks.
    /// </summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The exam identifier as it appears in the uploaded XML (e.g. "1").
    /// </summary>
    public string ExamId { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Percentage score — number of correct tasks divided by total tasks, multiplied by 100.
    /// </summary>
    public decimal Score { get; set; }

    public ICollection<ExamTask> Tasks { get; set; } = new List<ExamTask>();
}
