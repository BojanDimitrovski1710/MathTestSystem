namespace MathTestSystem.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }

    /// <summary>
    /// The teacher identifier as it appears in the uploaded XML (e.g. "11111").
    /// </summary>
    public string TeacherId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
