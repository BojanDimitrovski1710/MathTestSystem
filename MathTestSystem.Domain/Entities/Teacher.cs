namespace MathTestSystem.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }

    /// <summary>
    /// Publicly exposed identifier used in API routes.
    /// Generated on creation — prevents sequential ID enumeration attacks.
    /// </summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
