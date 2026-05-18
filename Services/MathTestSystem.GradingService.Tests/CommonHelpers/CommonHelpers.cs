using System.Xml.Linq;

namespace MathTestSystem.GradingService.Tests;

internal static class CommonHelpers
{
    internal static string BuildXml(string teacherId, IReadOnlyList<StudentXmlDto> students)
    {
        XElement xml = new("Teacher",
            new XAttribute("ID", teacherId),
            new XElement("Students",
                students.Select(student =>
                    new XElement("Student",
                        new XAttribute("ID", student.StudentId),
                        student.Exams.Select(exam =>
                            new XElement("Exam",
                                new XAttribute("Id", exam.ExamId),
                                exam.Tasks.Select(task =>
                                    new XElement("Task",
                                        new XAttribute("id", task.TaskId),
                                        task.Answer is null
                                            ? task.Expression
                                            : $"{task.Expression} = {task.Answer}"
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );

        return xml.ToString();
    }

    internal static string BuildXmlWithoutTeacherId() =>
        new XElement("Teacher",
            new XElement("Students",
                new XElement("Student", new XAttribute("ID", "12345"),
                    new XElement("Exam", new XAttribute("Id", "1"),
                        new XElement("Task", new XAttribute("id", "1"), "2+3 = 5"))))).ToString();

    internal static string BuildXmlWithoutStudentsElement() =>
        new XElement("Teacher", new XAttribute("ID", "11111")).ToString();

    internal static string BuildXmlWithoutStudentId() =>
        new XElement("Teacher", new XAttribute("ID", "11111"),
            new XElement("Students",
                new XElement("Student",
                    new XElement("Exam", new XAttribute("Id", "1"),
                        new XElement("Task", new XAttribute("id", "1"), "2+3 = 5"))))).ToString();

    internal static string BuildXmlWithoutExamId() =>
        new XElement("Teacher", new XAttribute("ID", "11111"),
            new XElement("Students",
                new XElement("Student", new XAttribute("ID", "12345"),
                    new XElement("Exam",
                        new XElement("Task", new XAttribute("id", "1"), "2+3 = 5"))))).ToString();

    internal static string BuildXmlWithoutTaskId() =>
        new XElement("Teacher", new XAttribute("ID", "11111"),
            new XElement("Students",
                new XElement("Student", new XAttribute("ID", "12345"),
                    new XElement("Exam", new XAttribute("Id", "1"),
                        new XElement("Task", "2+3 = 5"))))).ToString();
}
