namespace MathTestSystem.GradingService.Tests;

internal sealed record StudentXmlDto(string StudentId, IReadOnlyList<ExamXmlDto> Exams);
