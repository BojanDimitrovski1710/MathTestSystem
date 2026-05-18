namespace MathTestSystem.GradingService.Tests;

/// <param name="Answer">Pass <c>null</c> to produce a task with no '=' sign (tests the missing separator error path).</param>
internal sealed record TaskXmlDto(string TaskId, string Expression, string? Answer = null);
