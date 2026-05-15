namespace MathTestSystem.MathProcessor.Models;

public class EvaluationResult
{
    public bool Success { get; private set; }
    public decimal Value { get; private set; }
    public string? ErrorMessage { get; private set; }

    private EvaluationResult() { }

    public static EvaluationResult Ok(decimal value) =>
        new() { Success = true, Value = value };

    public static EvaluationResult Fail(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
