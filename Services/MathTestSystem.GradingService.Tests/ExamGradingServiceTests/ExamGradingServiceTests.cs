using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.GradingService.Models;
using MathTestSystem.GradingService.Parsing;
using MathTestSystem.GradingService.Services;
using MathTestSystem.Infrastructure.Data;
using MathTestSystem.MathProcessor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MathTestSystem.GradingService.Tests;

public class ExamGradingServiceTests
{
    private readonly ITeacherRepository _teacherRepo = Substitute.For<ITeacherRepository>();
    private readonly IStudentRepository _studentRepo = Substitute.For<IStudentRepository>();
    private readonly IExamRepository _examRepo = Substitute.For<IExamRepository>();
    private readonly ExamGradingService _service;

    public ExamGradingServiceTests()
    {
        ExamXmlParser parser = new();
        ExpressionEvaluator evaluator = new();

        UserManager<AppUser> userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        RoleManager<IdentityRole> roleManager = Substitute.For<RoleManager<IdentityRole>>(
            Substitute.For<IRoleStore<IdentityRole>>(),
            null, null, null, null);

        userManager.Users.Returns(new List<AppUser>().AsQueryable());
        userManager.FindByNameAsync(Arg.Any<string>()).Returns((AppUser?)null);
        userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(IdentityResult.Success);

        ILogger<ExamGradingService> logger = Substitute.For<ILogger<ExamGradingService>>();

        _service = new ExamGradingService(parser, evaluator, _teacherRepo, _studentRepo, _examRepo, userManager,
            roleManager, logger);

        // Batch existence checks — default to empty (nothing pre-exists)
        _teacherRepo.GetExistingIdsAsync(Arg.Any<IEnumerable<string>>()).Returns(new HashSet<string>());
        _studentRepo.GetExistingIdsAsync(Arg.Any<IEnumerable<string>>()).Returns(new HashSet<string>());
        _studentRepo.GetByStudentIdsAsync(Arg.Any<IEnumerable<string>>()).Returns(new List<Student>());

        // Fetch-by-id for existing entities
        _teacherRepo.GetByTeacherIdAsync(Arg.Any<string>()).Returns((Teacher?)null);

        // Single-entity saves
        _teacherRepo.AddAsync(Arg.Any<Teacher>()).Returns(call =>
        {
            Teacher t = call.Arg<Teacher>();
            t.Id = 1;
            return Task.FromResult(t);
        });

        // Bulk saves — assign IDs so studentMap is populated correctly
        _studentRepo.AddRangeAsync(Arg.Any<IEnumerable<Student>>()).Returns(call =>
        {
            int id = 1;
            foreach (Student s in call.Arg<IEnumerable<Student>>())
                s.Id = id++;
            return Task.CompletedTask;
        });

        _examRepo.AddRangeAsync(Arg.Any<IEnumerable<Exam>>()).Returns(Task.CompletedTask);
    }

    // -------------------------------------------------------------------------
    // Grading — correct and incorrect answers
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GradeAsync_CorrectAnswer_TaskIsMarkedCorrect()
    {
        string xml = ExamGradingServiceTestHelpers.BuildSingleTaskXml("6*2+3-4", "11");

        GradeExamResponse response = await _service.GradeAsync(xml);
        TaskGradeResult task = response.Students[0].Exams[0].Tasks[0];

        Assert.True(task.IsCorrect);
        Assert.False(task.HasError);
        Assert.Equal(11m, task.CorrectAnswer);
    }

    [Fact]
    public async Task GradeAsync_IncorrectAnswer_TaskIsMarkedIncorrect()
    {
        string xml = ExamGradingServiceTestHelpers.BuildSingleTaskXml("6*2+3-4", "22");

        GradeExamResponse response = await _service.GradeAsync(xml);
        TaskGradeResult task = response.Students[0].Exams[0].Tasks[0];

        Assert.False(task.IsCorrect);
        Assert.False(task.HasError);
        Assert.Equal(11m, task.CorrectAnswer);
        Assert.Equal(22m, task.StudentAnswer);
    }

    // -------------------------------------------------------------------------
    // Score calculation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GradeAsync_AllTasksCorrect_ScoreIsHundred()
    {
        string xml = ExamGradingServiceTestHelpers.BuildXml(
        [
            new SimpleTaskDto("2+3", "5"),
            new SimpleTaskDto("6*2", "12")
        ]);

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Equal(100m, response.Students[0].Exams[0].Score);
    }

    [Fact]
    public async Task GradeAsync_NoTasksCorrect_ScoreIsZero()
    {
        string xml = ExamGradingServiceTestHelpers.BuildXml(
        [
            new SimpleTaskDto("2+3", "99"),
            new SimpleTaskDto("6*2", "99")
        ]);

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Equal(0m, response.Students[0].Exams[0].Score);
    }

    [Fact]
    public async Task GradeAsync_HalfTasksCorrect_ScoreIsFifty()
    {
        string xml = ExamGradingServiceTestHelpers.BuildXml(
        [
            new SimpleTaskDto("2+3", "5"),
            new SimpleTaskDto("6*2", "99")
        ]);

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Equal(50m, response.Students[0].Exams[0].Score);
    }

    // -------------------------------------------------------------------------
    // Error handling — bad expressions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GradeAsync_InvalidExpression_TaskHasErrorAndNoCorrectAnswer()
    {
        string xml = ExamGradingServiceTestHelpers.BuildSingleTaskXml("5/0", "99");

        GradeExamResponse response = await _service.GradeAsync(xml);
        TaskGradeResult task = response.Students[0].Exams[0].Tasks[0];

        Assert.True(task.HasError);
        Assert.False(task.IsCorrect);
        Assert.Null(task.CorrectAnswer);
        Assert.NotNull(task.ErrorCode);
    }

    [Fact]
    public async Task GradeAsync_ErrorTasksExcludedFromScoreDenominator()
    {
        // 1 correct, 1 error — score should be 100 (1/1 gradable), not 50 (1/2 total)
        string xml = ExamGradingServiceTestHelpers.BuildXml(
        [
            new SimpleTaskDto("2+3", "5"),
            new SimpleTaskDto("5/0", "99")
        ]);

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Equal(100m, response.Students[0].Exams[0].Score);
    }

    // -------------------------------------------------------------------------
    // Assignment example — both tasks from the spec are incorrect
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GradeAsync_AssignmentExample_BothTasksIncorrect()
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

        GradeExamResponse response = await _service.GradeAsync(xml);
        IReadOnlyList<TaskGradeResult> tasks = response.Students[0].Exams[0].Tasks;

        Assert.False(tasks[0].IsCorrect);
        Assert.Equal(-1.5m, tasks[0].CorrectAnswer);
        Assert.Equal(74m, tasks[0].StudentAnswer);

        Assert.False(tasks[1].IsCorrect);
        Assert.Equal(11m, tasks[1].CorrectAnswer);
        Assert.Equal(22m, tasks[1].StudentAnswer);

        Assert.Equal(0m, response.Students[0].Exams[0].Score);
    }

    // -------------------------------------------------------------------------
    // Multiple exams and repeated students
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GradeAsync_StudentWithMultipleExams_AllExamsGraded()
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

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Single(response.Students);
        Assert.Equal(3, response.Students[0].Exams.Count);
        Assert.All(response.Students[0].Exams, e => Assert.Equal(100m, e.Score));
    }

    [Fact]
    public async Task GradeAsync_SameStudentAppearsMultipleTimes_NotDuplicated()
    {
        string xml = CommonHelpers.BuildXml("11111",
        [
            new StudentXmlDto("12345", [new ExamXmlDto("1", [new TaskXmlDto("1", "2+3", "5")])]),
            new StudentXmlDto("12345", [new ExamXmlDto("2", [new TaskXmlDto("1", "6*2", "12")])])
        ]);

        await _service.GradeAsync(xml);

        // Distinct() on studentIds means only one student entry is ever saved
        await _studentRepo.Received(1).AddRangeAsync(Arg.Is<IEnumerable<Student>>(s => s.Count() == 1));
    }

    // -------------------------------------------------------------------------
    // Teacher / student persistence
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GradeAsync_TeacherNotInDb_CreatesNewTeacher()
    {
        string xml = ExamGradingServiceTestHelpers.BuildSingleTaskXml("2+3", "5");

        await _service.GradeAsync(xml);

        await _teacherRepo.Received(1).AddAsync(Arg.Is<Teacher>(t => t.TeacherId == "11111"));
    }

    [Fact]
    public async Task GradeAsync_StudentAlreadyInDb_DoesNotCreateDuplicate()
    {
        Student existing = new("12345", 1) { Id = 1 };
        _studentRepo.GetExistingIdsAsync(Arg.Any<IEnumerable<string>>()).Returns(new HashSet<string> { "12345" });
        _studentRepo.GetByStudentIdsAsync(Arg.Any<IEnumerable<string>>()).Returns(new List<Student> { existing });

        string xml = ExamGradingServiceTestHelpers.BuildSingleTaskXml("2+3", "5");

        await _service.GradeAsync(xml);

        await _studentRepo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Student>>());
    }

    [Fact]
    public async Task GradeAsync_OneOfThreeTasksCorrect_ScoreRoundsToTwoDecimalPlaces()
    {
        string xml = ExamGradingServiceTestHelpers.BuildXml(
        [
            new SimpleTaskDto("2+3", "5"),   // correct
            new SimpleTaskDto("2+3", "99"),  // wrong
            new SimpleTaskDto("2+3", "99")   // wrong
        ]);

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Equal(33.33m, response.Students[0].Exams[0].Score);
    }

    [Fact]
    public async Task GradeAsync_AllTasksHaveErrors_ScoreIsZero()
    {
        string xml = ExamGradingServiceTestHelpers.BuildXml(
        [
            new SimpleTaskDto("5/0", "99"),
            new SimpleTaskDto("5/0", "99")
        ]);

        GradeExamResponse response = await _service.GradeAsync(xml);

        Assert.Equal(0m, response.Students[0].Exams[0].Score);
        Assert.All(response.Students[0].Exams[0].Tasks, t => Assert.True(t.HasError));
    }

    [Fact]
    public async Task GradeAsync_TeacherAlreadyInDb_DoesNotCreateDuplicate()
    {
        Teacher existing = new("11111") { Id = 1 };
        _teacherRepo.GetExistingIdsAsync(Arg.Any<IEnumerable<string>>()).Returns(new HashSet<string> { "11111" });
        _teacherRepo.GetByTeacherIdAsync("11111").Returns(existing);

        string xml = ExamGradingServiceTestHelpers.BuildSingleTaskXml("2+3", "5");

        await _service.GradeAsync(xml);

        await _teacherRepo.DidNotReceive().AddAsync(Arg.Any<Teacher>());
    }
}
