namespace MathTestSystem.GradingService.Tests;

internal sealed record ExamXmlDto(string ExamId, IReadOnlyList<TaskXmlDto> Tasks);
