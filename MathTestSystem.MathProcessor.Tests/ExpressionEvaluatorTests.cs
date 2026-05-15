using MathTestSystem.Domain.Constants;
using MathTestSystem.MathProcessor.Models;
using MathTestSystem.MathProcessor.Services;

namespace MathTestSystem.MathProcessor.Tests;

public class ExpressionEvaluatorTests
{
    private readonly ExpressionEvaluator _evaluator = new();

    // -------------------------------------------------------------------------
    // Basic operations
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("2+3", 5)]
    [InlineData("10-4", 6)]
    [InlineData("3*4", 12)]
    [InlineData("10/2", 5)]
    public void Evaluate_BasicOperations_ReturnsCorrectResult(string expression, decimal expected)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.True(result.Success);
        Assert.Equal(expected, result.Value);
    }

    // -------------------------------------------------------------------------
    // Operator precedence — multiplication and division before addition/subtraction
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("2+3*4", 14)]       // not 20
    [InlineData("10-2*3", 4)]       // not 24
    [InlineData("8/2+3", 7)]        // not 2
    [InlineData("2+10/2-1", 6)]     // 2+5-1
    public void Evaluate_OperatorPrecedence_MultiplicationBeforeAddition(string expression, decimal expected)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.True(result.Success);
        Assert.Equal(expected, result.Value);
    }

    // -------------------------------------------------------------------------
    // Parentheses override precedence
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("(2+3)*4", 20)]
    [InlineData("(10-4)*2", 12)]
    [InlineData("10/(2+3)", 2)]
    [InlineData("(2+3)*(4-1)", 15)]
    public void Evaluate_Parentheses_OverridePrecedence(string expression, decimal expected)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.True(result.Success);
        Assert.Equal(expected, result.Value);
    }

    // -------------------------------------------------------------------------
    // Decimal results
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("1/2", 0.5)]
    [InlineData("2+3/6-4", -1.5)]   // 3/6=0.5, 2+0.5-4=-1.5
    [InlineData("7/2", 3.5)]
    public void Evaluate_DecimalResults_ReturnsCorrectDecimal(string expression, decimal expected)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.True(result.Success);
        Assert.Equal(expected, result.Value);
    }

    // -------------------------------------------------------------------------
    // Unary minus
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("-5+3", -2)]
    [InlineData("-5*2", -10)]
    [InlineData("-(2+3)", -5)]
    public void Evaluate_UnaryMinus_HandledCorrectly(string expression, decimal expected)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.True(result.Success);
        Assert.Equal(expected, result.Value);
    }

    // -------------------------------------------------------------------------
    // Whitespace is ignored
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_ExpressionWithSpaces_IgnoresWhitespace()
    {
        EvaluationResult result = _evaluator.Evaluate("2 + 3 * 4");
        Assert.True(result.Success);
        Assert.Equal(14, result.Value);
    }

    // -------------------------------------------------------------------------
    // Assignment XML examples — proves grading logic is correct
    // Task 1: 2+3/6-4 = 74  (student answered 74, correct answer is -1.5)
    // Task 2: 6*2+3-4 = 22  (student answered 22, correct answer is 11)
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_AssignmentTask1_CorrectAnswerIsNotStudentAnswer()
    {
        EvaluationResult result = _evaluator.Evaluate("2+3/6-4");
        Assert.True(result.Success);
        Assert.Equal(-1.5m, result.Value);
        Assert.NotEqual(74m, result.Value); // student was wrong
    }

    [Fact]
    public void Evaluate_AssignmentTask2_CorrectAnswerIsNotStudentAnswer()
    {
        EvaluationResult result = _evaluator.Evaluate("6*2+3-4");
        Assert.True(result.Success);
        Assert.Equal(11m, result.Value);
        Assert.NotEqual(22m, result.Value); // student was wrong
    }

    // -------------------------------------------------------------------------
    // Error cases
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_DivisionByZero_ReturnsFailure()
    {
        EvaluationResult result = _evaluator.Evaluate("5/0");
        Assert.False(result.Success);
        Assert.Equal(ResultCodes.ExpressionDivisionByZero, result.ErrorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Evaluate_EmptyExpression_ReturnsFailure(string expression)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.False(result.Success);
    }

    [Theory]
    [InlineData("2++3")]
    [InlineData("(2+3")]
    [InlineData("2+3)")]
    [InlineData("2+*3")]
    public void Evaluate_MalformedExpression_ReturnsFailure(string expression)
    {
        EvaluationResult result = _evaluator.Evaluate(expression);
        Assert.False(result.Success);
    }
}
