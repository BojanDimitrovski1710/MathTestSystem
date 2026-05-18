using MathTestSystem.GradingService.Models;

namespace MathTestSystem.GradingService.Services;

/// <summary>
/// Orchestrates XML parsing, expression evaluation, persistence, and result assembly.
/// </summary>
public interface IGradingService
{
    /// <summary>
    /// Parses and grades a teacher-uploaded XML submission, persists the results,
    /// and returns a structured grading response.
    /// </summary>
    Task<GradeExamResponse> GradeAsync(string xml);
}
