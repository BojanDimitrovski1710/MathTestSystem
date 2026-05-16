namespace MathTestSystem.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    /// <summary>
    /// Publicly exposed identifier used in API routes.
    /// Generated on creation — prevents sequential ID enumeration attacks.
    /// </summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The student identifier as it appears in the uploaded XML (e.g. "12345").
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
