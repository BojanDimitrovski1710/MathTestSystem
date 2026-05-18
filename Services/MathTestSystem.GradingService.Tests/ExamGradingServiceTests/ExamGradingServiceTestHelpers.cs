namespace MathTestSystem.GradingService.Tests;

internal static class ExamGradingServiceTestHelpers
{
    /// <summary>
    /// Builds XML for a single student (12345) with a single exam (1).
    /// Teacher ID is fixed to "11111".
    /// </summary>
    internal static string BuildXml(IReadOnlyList<SimpleTaskDto> tasks)
    {
        int taskId = 1;
        return CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345",
            [
                new ExamXmlDto("1",
                    tasks.Select(t => new TaskXmlDto(taskId++.ToString(), t.Expression, t.Answer)).ToList())
            ])
        ]);
    }

    internal static string BuildSingleTaskXml(string expression, string answer) =>
        BuildXml([new SimpleTaskDto(expression, answer)]);
}
