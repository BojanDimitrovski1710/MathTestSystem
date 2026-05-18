namespace MathTestSystem.Domain.Entities;

public class Teacher
{
    public Teacher(string teacherId)
    {
        TeacherId = teacherId;
    }

    private Teacher() { } // EF Core

    public int Id { get; set; }

    /// <summary>
    /// Publicly exposed identifier used in API routes.
    /// Generated on creation — prevents sequential ID enumeration attacks.
    /// </summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The teacher identifier as it appears in the uploaded XML (e.g. "11111").
    /// </summary>
    public string TeacherId { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
