namespace MathTestSystem.GradingService.Parsing;

/// <summary>
/// Parses the teacher-uploaded XML document into an intermediate model.
/// </summary>
public interface IExamXmlParser
{
    /// <summary>
    /// Parses a raw XML string conforming to the exam submission schema.
    /// </summary>
    /// <param name="xml">The raw XML content uploaded by the teacher.</param>
    /// <returns>A <see cref="ParsedTeacherExam"/> containing all students and their tasks.</returns>
    ParsedTeacherExam Parse(string xml);
}
