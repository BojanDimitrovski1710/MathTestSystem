namespace MathTestSystem.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    /// <summary>
    /// The student identifier as it appears in the uploaded XML (e.g. "12345").
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
