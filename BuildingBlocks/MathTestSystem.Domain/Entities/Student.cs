namespace MathTestSystem.Domain.Entities;

public class Student
{
    public Student(string studentId, int teacherId)
    {
        StudentId = studentId;
        TeacherId = teacherId;
    }

    private Student() { } // EF Core

    public int Id { get; set; }

    /// <summary>
    /// Publicly exposed identifier used in API routes.
    /// Generated on creation — prevents sequential ID enumeration attacks.
    /// </summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The student identifier as it appears in the uploaded XML (e.g. "12345").
    /// </summary>
    public string StudentId { get; set; }

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
