using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using MathTestSystem.Domain.Constants;

namespace MathTestSystem.GradingService.Parsing;

public class ExamXmlParser : IExamXmlParser
{
    private static readonly XmlSchemaSet SchemaSet = LoadSchema();

    public ParsedTeacherExam Parse(string xml)
    {
        XElement root = Validate(XDocument.Parse(xml));

        string teacherId = root.Attribute("ID")?.Value
            ?? throw new InvalidOperationException(ResultCodes.XmlTeacherIdMissing);

        XElement studentsElement = root.Element("Students")
            ?? throw new InvalidOperationException(ResultCodes.XmlStudentsMissing);

        IReadOnlyList<ParsedStudent> students = studentsElement
            .Elements("Student")
            .Select(ParseStudent)
            .ToList();

        return new ParsedTeacherExam(teacherId, students);
    }

    // -------------------------------------------------------------------------
    // Schema validation
    // -------------------------------------------------------------------------

    private static XElement Validate(XDocument doc)
    {
        List<string> errors = [];

        doc.Validate(SchemaSet, (_, e) => errors.Add(e.Message));

        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"{ResultCodes.XmlSchemaValidationFailed}: {string.Join("; ", errors)}");

        return doc.Root
            ?? throw new InvalidOperationException(ResultCodes.XmlRootMissing);
    }

    private static XmlSchemaSet LoadSchema()
    {
        const string resourceName = "MathTestSystem.GradingService.Schemas.TeacherExam.xsd";

        using Stream stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(ResultCodes.XmlSchemaNotFound);

        XmlSchemaSet set = new();
        set.Add(null, System.Xml.XmlReader.Create(stream));
        return set;
    }

    // -------------------------------------------------------------------------
    // Parsing
    // -------------------------------------------------------------------------

    private static ParsedStudent ParseStudent(XElement student) =>
        new(
            StudentId: student.Attribute("ID")?.Value
                ?? throw new InvalidOperationException(ResultCodes.XmlStudentIdMissing),
            Exams: student
                .Elements("Exam")
                .Select(ParseExam)
                .ToList());

    private static ParsedExam ParseExam(XElement exam) =>
        new(
            ExamId: exam.Attribute("Id")?.Value
                ?? throw new InvalidOperationException(ResultCodes.XmlExamIdMissing),
            Tasks: exam
                .Elements("Task")
                .Select(ParseTask)
                .ToList());

    private static ParsedTask ParseTask(XElement task)
    {
        string taskId = task.Attribute("id")?.Value
            ?? throw new InvalidOperationException(ResultCodes.XmlTaskIdMissing);

        string content = task.Value.Trim();
        int equalsIndex = content.LastIndexOf('=');

        if (equalsIndex < 0)
            throw new InvalidOperationException(ResultCodes.XmlTaskMissingEqualsSeparator);

        string expression = content[..equalsIndex].Trim();
        string answerStr = content[(equalsIndex + 1)..].Trim();

        if (!decimal.TryParse(answerStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal studentAnswer))
            throw new InvalidOperationException(ResultCodes.XmlTaskInvalidStudentAnswer);

        return new ParsedTask(taskId, expression, studentAnswer);
    }
}
