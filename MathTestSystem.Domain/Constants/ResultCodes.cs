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

    #region Xml

    public const string XmlSchemaNotFound = "XML_SCHEMA_NOT_FOUND";
    public const string XmlSchemaValidationFailed = "XML_SCHEMA_VALIDATION_FAILED";
    public const string XmlRootMissing = "XML_ROOT_MISSING";
    public const string XmlTeacherIdMissing = "XML_TEACHER_ID_MISSING";
    public const string XmlStudentsMissing = "XML_STUDENTS_MISSING";
    public const string XmlStudentIdMissing = "XML_STUDENT_ID_MISSING";
    public const string XmlExamIdMissing = "XML_EXAM_ID_MISSING";
    public const string XmlTaskIdMissing = "XML_TASK_ID_MISSING";
    public const string XmlTaskMissingEqualsSeparator = "XML_TASK_MISSING_EQUALS_SEPARATOR";
    public const string XmlTaskInvalidStudentAnswer = "XML_TASK_INVALID_STUDENT_ANSWER";

    #endregion

    #region Repository

    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string ExamNotFound = "EXAM_NOT_FOUND";

    #endregion

    #region Api

    public const string RequestBodyEmpty = "REQUEST_BODY_EMPTY";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";

    #endregion
}
