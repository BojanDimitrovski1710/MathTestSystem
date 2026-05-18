using MathTestSystem.Domain.Constants;
using MathTestSystem.GradingService.Parsing;

namespace MathTestSystem.GradingService.Tests;

public class ExamXmlParserTests
{
    private readonly ExamXmlParser _parser = new();

    // -------------------------------------------------------------------------
    // Valid XML — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_ValidXml_ReturnsCorrectTeacherId()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345",
            [
                new ExamXmlDto("1", [new TaskXmlDto("1", "2+3", "5")])
            ])
        ]);

        ParsedTeacherExam result = _parser.Parse(xml);

        Assert.Equal("11111", result.TeacherId);
    }

    [Fact]
    public void Parse_ValidXml_ReturnsCorrectStudentAndExamStructure()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345",
            [
                new ExamXmlDto("1",
                [
                    new TaskXmlDto("1", "2+3", "5"),
                    new TaskXmlDto("2", "6*2", "12")
                ])
            ])
        ]);

        ParsedTeacherExam result = _parser.Parse(xml);

        Assert.Single(result.Students);
        Assert.Equal("12345", result.Students[0].StudentId);
        Assert.Single(result.Students[0].Exams);
        Assert.Equal("1", result.Students[0].Exams[0].ExamId);
        Assert.Equal(2, result.Students[0].Exams[0].Tasks.Count);
    }

    [Fact]
    public void Parse_MultipleStudents_ReturnsAllStudents()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345", [new ExamXmlDto("1", [new TaskXmlDto("1", "2+3", "5")])]),
            new StudentXmlDto("54321", [new ExamXmlDto("1", [new TaskXmlDto("1", "2+3", "5")])])
        ]);

        ParsedTeacherExam result = _parser.Parse(xml);

        Assert.Equal(2, result.Students.Count);
        Assert.Equal("12345", result.Students[0].StudentId);
        Assert.Equal("54321", result.Students[1].StudentId);
    }

    [Fact]
    public void Parse_StudentWithMultipleExams_ReturnsAllExams()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("54321",
            [
                new ExamXmlDto("1", [new TaskXmlDto("1", "2+3", "5")]),
                new ExamXmlDto("2", [new TaskXmlDto("1", "6*2", "12")]),
                new ExamXmlDto("3", [new TaskXmlDto("1", "10-4", "6")])
            ])
        ]);

        ParsedTeacherExam result = _parser.Parse(xml);

        Assert.Single(result.Students);
        Assert.Equal(3, result.Students[0].Exams.Count);
    }

    [Fact]
    public void Parse_TaskExpression_ParsesExpressionAndAnswerCorrectly()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345",
            [
                new ExamXmlDto("1", [new TaskXmlDto("1", "2+3/6-4", "74")])
            ])
        ]);

        ParsedTeacherExam result = _parser.Parse(xml);
        ParsedTask task = result.Students[0].Exams[0].Tasks[0];

        Assert.Equal("1", task.TaskId);
        Assert.Equal("2+3/6-4", task.Expression);
        Assert.Equal(74m, task.StudentAnswer);
    }

    [Fact]
    public void Parse_AssignmentExample_ParsesBothTasksCorrectly()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345",
            [
                new ExamXmlDto("1",
                [
                    new TaskXmlDto("1", "2+3/6-4", "74"),
                    new TaskXmlDto("2", "6*2+3-4", "22")
                ])
            ])
        ]);

        ParsedTeacherExam result = _parser.Parse(xml);
        IReadOnlyList<ParsedTask> tasks = result.Students[0].Exams[0].Tasks;

        Assert.Equal(2, tasks.Count);
        Assert.Equal("2+3/6-4", tasks[0].Expression);
        Assert.Equal(74m, tasks[0].StudentAnswer);
        Assert.Equal("6*2+3-4", tasks[1].Expression);
        Assert.Equal(22m, tasks[1].StudentAnswer);
    }

    // -------------------------------------------------------------------------
    // Malformed XML — each missing field returns the correct result code
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_MissingTeacherId_ThrowsWithCorrectCode()
    {
        string xml = CommonHelpers.BuildXmlWithoutTeacherId();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.StartsWith(ResultCodes.XmlSchemaValidationFailed, ex.Message);
    }

    [Fact]
    public void Parse_MissingStudentsElement_ThrowsWithCorrectCode()
    {
        string xml = CommonHelpers.BuildXmlWithoutStudentsElement();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.StartsWith(ResultCodes.XmlSchemaValidationFailed, ex.Message);
    }

    [Fact]
    public void Parse_MissingStudentId_ThrowsWithCorrectCode()
    {
        string xml = CommonHelpers.BuildXmlWithoutStudentId();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.StartsWith(ResultCodes.XmlSchemaValidationFailed, ex.Message);
    }

    [Fact]
    public void Parse_MissingExamId_ThrowsWithCorrectCode()
    {
        string xml = CommonHelpers.BuildXmlWithoutExamId();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.StartsWith(ResultCodes.XmlSchemaValidationFailed, ex.Message);
    }

    [Fact]
    public void Parse_MissingTaskId_ThrowsWithCorrectCode()
    {
        string xml = CommonHelpers.BuildXmlWithoutTaskId();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.StartsWith(ResultCodes.XmlSchemaValidationFailed, ex.Message);
    }

    [Fact]
    public void Parse_TaskMissingEqualsSeparator_ThrowsWithCorrectCode()
    {
        // TaskXmlDto with null Answer produces a task with no '=' sign
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345", [new ExamXmlDto("1", [new TaskXmlDto("1", "2+3")])])
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.Equal(ResultCodes.XmlTaskMissingEqualsSeparator, ex.Message);
    }

    [Fact]
    public void Parse_TaskInvalidStudentAnswer_ThrowsWithCorrectCode()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345", [new ExamXmlDto("1", [new TaskXmlDto("1", "2+3", "abc")])])
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.Equal(ResultCodes.XmlTaskInvalidStudentAnswer, ex.Message);
    }

    [Fact]
    public void Parse_CompletelyInvalidXml_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _parser.Parse("this is not xml at all"));
    }

    [Fact]
    public void Parse_EmptyStudentsList_ThrowsWithCorrectCode()
    {
        // The XSD requires at least one Student — an empty list is schema-invalid.
        string xml = CommonHelpers.BuildXml("11111", []);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _parser.Parse(xml));
        Assert.StartsWith(ResultCodes.XmlSchemaValidationFailed, ex.Message);
    }
}
