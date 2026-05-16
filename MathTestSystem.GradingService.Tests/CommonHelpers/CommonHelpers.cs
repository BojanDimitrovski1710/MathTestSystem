using System.Text;

namespace MathTestSystem.GradingService.Tests;

internal static class CommonHelpers
{
    internal static string BuildXml(string teacherId, IReadOnlyList<StudentXmlDto> students)
    {
        StringBuilder sb = new();
        sb.AppendLine($"<Teacher ID=\"{teacherId}\">");
        sb.AppendLine("  <Students>");

        foreach (StudentXmlDto student in students)
        {
            sb.AppendLine($"    <Student ID=\"{student.StudentId}\">");

            foreach (ExamXmlDto exam in student.Exams)
            {
                sb.AppendLine($"      <Exam Id=\"{exam.ExamId}\">");

                foreach (TaskXmlDto task in exam.Tasks)
                {
                    string content = task.Answer is null
                        ? task.Expression
                        : $"{task.Expression} = {task.Answer}";
                    sb.AppendLine($"        <Task id=\"{task.TaskId}\"> {content} </Task>");
                }

                sb.AppendLine("      </Exam>");
            }

            sb.AppendLine("    </Student>");
        }

        sb.AppendLine("  </Students>");
        sb.AppendLine("</Teacher>");
        return sb.ToString();
    }
}
