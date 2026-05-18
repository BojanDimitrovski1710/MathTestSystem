using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using MathTestSystem.Domain.Constants;

namespace MathTestSystem.GradingService.Parsing;

public class ExamXmlParser : IExamXmlParser
{
    private static readonly XmlSchemaSet SchemaSet = LoadSchema();

    public ParsedTeacherExam Parse(string xml)
    {
        TeacherXml teacherXml = XmlProcessor.DeserializeWithValidation<TeacherXml>(xml, SchemaSet);
        return MapTeacher(teacherXml);
    }

    // -------------------------------------------------------------------------
    // Mapping — TeacherXml → domain parsed result
    // -------------------------------------------------------------------------

    private static ParsedTeacherExam MapTeacher(TeacherXml teacherXml) =>
        new(teacherXml.ID, teacherXml.Students.Select(MapStudent).ToList());

    private static ParsedStudent MapStudent(StudentXml student) =>
        new(student.ID, student.Exams.Select(MapExam).ToList());

    private static ParsedExam MapExam(ExamXml exam) =>
        new(exam.Id, exam.Tasks.Select(MapTask).ToList());

    private static ParsedTask MapTask(TaskXml task)
    {
        string content = task.Content.Trim();
        int equalsIndex = content.LastIndexOf('=');

        if (equalsIndex < 0)
            throw new InvalidOperationException(ResultCodes.XmlTaskMissingEqualsSeparator);

        string expression = content[..equalsIndex].Trim();
        string answerStr = content[(equalsIndex + 1)..].Trim();

        if (!decimal.TryParse(answerStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal studentAnswer))
            throw new InvalidOperationException(ResultCodes.XmlTaskInvalidStudentAnswer);

        return new ParsedTask(task.Id, expression, studentAnswer);
    }

    // -------------------------------------------------------------------------
    // Schema — loaded once from embedded resource
    // -------------------------------------------------------------------------

    private static XmlSchemaSet LoadSchema()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = assembly
            .GetManifestResourceNames()
            .Single(n => n.EndsWith("TeacherExam.xsd"));

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(ResultCodes.XmlSchemaNotFound);

        XmlSchemaSet set = new();
        set.Add(null, XmlReader.Create(stream));
        return set;
    }
}
