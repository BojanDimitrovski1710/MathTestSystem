using MathTestSystem.MathProcessor.Models;

namespace MathTestSystem.MathProcessor.Interfaces;

/// <summary>
/// Independent processor for evaluating arithmetic expressions.
/// Implementations must respect standard operator precedence (PEMDAS).
/// </summary>
public interface IExpressionEvaluator
{
    /// <summary>
    /// Evaluates an arithmetic expression string and returns the result.
    /// </summary>
    /// <param name="expression">An arithmetic expression e.g. "2+3/6-4"</param>
    /// <returns>An EvaluationResult indicating success or failure with the computed value.</returns>
    EvaluationResult Evaluate(string expression);
}
