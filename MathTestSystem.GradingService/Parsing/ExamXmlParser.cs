using System.Xml.Linq;
using MathTestSystem.Domain.Constants;

namespace MathTestSystem.GradingService.Parsing;

public class ExamXmlParser : IExamXmlParser
{
    public ParsedTeacherExam Parse(string xml)
    {
        XDocument doc = XDocument.Parse(xml);
        XElement root = doc.Root ?? throw new InvalidOperationException(ResultCodes.XmlRootMissing);

        string teacherId = (string?)root.Attribute("ID")
            ?? throw new InvalidOperationException(ResultCodes.XmlTeacherIdMissing);

        XElement studentsElement = root.Element("Students")
            ?? throw new InvalidOperationException(ResultCodes.XmlStudentsMissing);

        IReadOnlyList<ParsedStudent> students = studentsElement
            .Elements("Student")
            .Select(ParseStudent)
            .ToList();

        return new ParsedTeacherExam(teacherId, students);
    }

    private static ParsedStudent ParseStudent(XElement studentElement)
    {
        string studentId = (string?)studentElement.Attribute("ID")
            ?? throw new InvalidOperationException(ResultCodes.XmlStudentIdMissing);

        IReadOnlyList<ParsedExam> exams = studentElement
            .Elements("Exam")
            .Select(ParseExam)
            .ToList();

        return new ParsedStudent(studentId, exams);
    }

    private static ParsedExam ParseExam(XElement examElement)
    {
        string examId = (string?)examElement.Attribute("Id")
            ?? throw new InvalidOperationException(ResultCodes.XmlExamIdMissing);

        IReadOnlyList<ParsedTask> tasks = examElement
            .Elements("Task")
            .Select(ParseTask)
            .ToList();

        return new ParsedExam(examId, tasks);
    }

    private static ParsedTask ParseTask(XElement taskElement)
    {
        string taskId = (string?)taskElement.Attribute("id")
            ?? throw new InvalidOperationException(ResultCodes.XmlTaskIdMissing);

        string content = taskElement.Value.Trim();
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
