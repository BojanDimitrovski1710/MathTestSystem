namespace MathTestSystem.Domain.Constants;

public static class ResultCodes
{
    #region Expression

    public const string ExpressionEmpty = "EXPRESSION_EMPTY";
    public const string ExpressionDivisionByZero = "EXPRESSION_DIVISION_BY_ZERO";
    public const string ExpressionInvalidCharacter = "EXPRESSION_INVALID_CHARACTER";
    public const string ExpressionMismatchedParentheses = "EXPRESSION_MISMATCHED_PARENTHESES";
    public const string ExpressionUnknownToken = "EXPRESSION_UNKNOWN_TOKEN";
    public const string ExpressionInsufficientOperands = "EXPRESSION_INSUFFICIENT_OPERANDS";
    public const string ExpressionInvalidStructure = "EXPRESSION_INVALID_STRUCTURE";
    public const string ExpressionEvaluationFailed = "EXPRESSION_EVALUATION_FAILED";

    #endregion

    #region Repository

    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string ExamNotFound = "EXAM_NOT_FOUND";

    #endregion
}
