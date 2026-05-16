using System.Xml.Linq;

namespace MathTestSystem.GradingService.Parsing;

public class ExamXmlParser : IExamXmlParser
{
    public ParsedTeacherExam Parse(string xml)
    {
        XDocument doc = XDocument.Parse(xml);
        XElement root = doc.Root ?? throw new InvalidOperationException("XML root element is missing.");

        string teacherId = (string?)root.Attribute("ID")
            ?? throw new InvalidOperationException("Teacher ID attribute is missing.");

        XElement studentsElement = root.Element("Students")
            ?? throw new InvalidOperationException("Students element is missing.");

        IReadOnlyList<ParsedStudent> students = studentsElement
            .Elements("Student")
            .Select(ParseStudent)
            .ToList();

        return new ParsedTeacherExam(teacherId, students);
    }

    private static ParsedStudent ParseStudent(XElement studentElement)
    {
        string studentId = (string?)studentElement.Attribute("ID")
            ?? throw new InvalidOperationException("Student ID attribute is missing.");

        IReadOnlyList<ParsedExam> exams = studentElement
            .Elements("Exam")
            .Select(ParseExam)
            .ToList();

        return new ParsedStudent(studentId, exams);
    }

    private static ParsedExam ParseExam(XElement examElement)
    {
        string examId = (string?)examElement.Attribute("Id")
            ?? throw new InvalidOperationException("Exam Id attribute is missing.");

        IReadOnlyList<ParsedTask> tasks = examElement
            .Elements("Task")
            .Select(ParseTask)
            .ToList();

        return new ParsedExam(examId, tasks);
    }

    private static ParsedTask ParseTask(XElement taskElement)
    {
        string taskId = (string?)taskElement.Attribute("id")
            ?? throw new InvalidOperationException("Task id attribute is missing.");

        string content = taskElement.Value.Trim();
        int equalsIndex = content.LastIndexOf('=');

        if (equalsIndex < 0)
            throw new InvalidOperationException($"Task {taskId} is missing '=' separator.");

        string expression = content[..equalsIndex].Trim();
        string answerStr = content[(equalsIndex + 1)..].Trim();

        if (!decimal.TryParse(answerStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal studentAnswer))
            throw new InvalidOperationException($"Task {taskId} has an invalid student answer: '{answerStr}'.");

        return new ParsedTask(taskId, expression, studentAnswer);
    }
}
