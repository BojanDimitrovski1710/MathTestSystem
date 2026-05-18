using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Controllers;
using MathTestSystem.StudentService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MathTestSystem.StudentService.Tests.Controllers;

public class StudentControllerTests
{
    private readonly IStudentRepository _studentRepo = Substitute.For<IStudentRepository>();
    private readonly IExamRepository _examRepo = Substitute.For<IExamRepository>();
    private readonly StudentController _controller;

    private static readonly Teacher DefaultTeacher = new("11111") { Id = 1 };
    private static readonly Student DefaultStudent = new("12345", 1)
    {
        Id = 1,
        Teacher = DefaultTeacher
    };

    public StudentControllerTests()
    {
        _controller = new StudentController(
            _studentRepo,
            _examRepo,
            Substitute.For<ILogger<StudentController>>());
    }

    // -------------------------------------------------------------------------
    // GetDashboard
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetDashboard_StudentNotFound_Returns404()
    {
        _studentRepo.GetByStudentIdAsync("99999").Returns((Student?)null);

        IActionResult result = await _controller.GetDashboard("99999");

        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(ResultCodes.StudentNotFound, notFound.Value);
    }

    [Fact]
    public async Task GetDashboard_NoExams_ReturnsZeroScore()
    {
        _studentRepo.GetByStudentIdAsync("12345").Returns(DefaultStudent);
        _examRepo.GetByStudentUidAsync(DefaultStudent.Uid).Returns([]);

        IActionResult result = await _controller.GetDashboard("12345");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        StudentDashboardResponse response = Assert.IsType<StudentDashboardResponse>(ok.Value);
        Assert.Equal(0m, response.OverallScore);
        Assert.Equal(0, response.OverallTotal);
    }

    [Fact]
    public async Task GetDashboard_WithExams_AggregatesScoreCorrectly()
    {
        _studentRepo.GetByStudentIdAsync("12345").Returns(DefaultStudent);

        Exam exam = BuildExam("1", [
            BuildTask("1", isCorrect: true),
            BuildTask("2", isCorrect: true),
            BuildTask("3", isCorrect: false)
        ]);

        _examRepo.GetByStudentUidAsync(DefaultStudent.Uid).Returns([exam]);

        IActionResult result = await _controller.GetDashboard("12345");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        StudentDashboardResponse response = Assert.IsType<StudentDashboardResponse>(ok.Value);
        Assert.Equal(2, response.OverallCorrect);
        Assert.Equal(3, response.OverallTotal);
        Assert.Equal(66.67m, response.OverallScore);
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectTeacherIdInResponse()
    {
        _studentRepo.GetByStudentIdAsync("12345").Returns(DefaultStudent);
        _examRepo.GetByStudentUidAsync(DefaultStudent.Uid).Returns([]);

        IActionResult result = await _controller.GetDashboard("12345");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        StudentDashboardResponse response = Assert.IsType<StudentDashboardResponse>(ok.Value);
        Assert.Single(response.Teachers);
        Assert.Equal("11111", response.Teachers[0].TeacherId);
    }

    // -------------------------------------------------------------------------
    // GetExams
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetExams_StudentNotFound_Returns404()
    {
        Guid uid = Guid.NewGuid();
        _studentRepo.GetByUidAsync(uid).Returns((Student?)null);

        IActionResult result = await _controller.GetExams(uid);

        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(ResultCodes.StudentNotFound, notFound.Value);
    }

    [Fact]
    public async Task GetExams_ReturnsAllExamsForStudent()
    {
        _studentRepo.GetByUidAsync(DefaultStudent.Uid).Returns(DefaultStudent);
        _examRepo.GetByStudentUidAsync(DefaultStudent.Uid).Returns([
            BuildExam("1", [BuildTask("1", isCorrect: true)]),
            BuildExam("2", [BuildTask("1", isCorrect: false)])
        ]);

        IActionResult result = await _controller.GetExams(DefaultStudent.Uid);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<ExamSummaryResponse> response = Assert.IsAssignableFrom<IReadOnlyList<ExamSummaryResponse>>(ok.Value);
        Assert.Equal(2, response.Count);
    }

    // -------------------------------------------------------------------------
    // GetExamDetail
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetExamDetail_StudentNotFound_Returns404()
    {
        Guid studentUid = Guid.NewGuid();
        _studentRepo.GetByUidAsync(studentUid).Returns((Student?)null);

        IActionResult result = await _controller.GetExamDetail(studentUid, Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetExamDetail_ExamNotFound_Returns404()
    {
        _studentRepo.GetByUidAsync(DefaultStudent.Uid).Returns(DefaultStudent);
        _examRepo.GetWithTasksAsync(Arg.Any<Guid>()).Returns((Exam?)null);

        IActionResult result = await _controller.GetExamDetail(DefaultStudent.Uid, Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetExamDetail_ExamBelongsToDifferentStudent_Returns404()
    {
        Student otherStudent = new("99999", 1) { Id = 2 };
        Exam exam = BuildExam("1", [BuildTask("1", isCorrect: true)]);
        exam.Student = otherStudent; // exam belongs to a different student

        _studentRepo.GetByUidAsync(DefaultStudent.Uid).Returns(DefaultStudent);
        _examRepo.GetWithTasksAsync(exam.Uid).Returns(exam);

        IActionResult result = await _controller.GetExamDetail(DefaultStudent.Uid, exam.Uid);

        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(ResultCodes.ExamNotFound, notFound.Value);
    }

    [Fact]
    public async Task GetExamDetail_ValidRequest_ReturnsExamWithTasks()
    {
        Exam exam = BuildExam("1", [
            BuildTask("1", isCorrect: true),
            BuildTask("2", isCorrect: false)
        ]);
        exam.Student = DefaultStudent;

        _studentRepo.GetByUidAsync(DefaultStudent.Uid).Returns(DefaultStudent);
        _examRepo.GetWithTasksAsync(exam.Uid).Returns(exam);

        IActionResult result = await _controller.GetExamDetail(DefaultStudent.Uid, exam.Uid);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        ExamDetailResponse response = Assert.IsType<ExamDetailResponse>(ok.Value);
        Assert.Equal("1", response.ExamId);
        Assert.Equal(2, response.Tasks.Count);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Exam BuildExam(string examId, List<ExamTask> tasks)
    {
        Exam exam = new(examId, 1, 1) { Tasks = tasks };
        int correct = tasks.Count(t => t.IsCorrect);
        int total = tasks.Count;
        exam.Score = total > 0 ? Math.Round((decimal)correct / total * 100, 2) : 0m;
        return exam;
    }

    private static ExamTask BuildTask(string taskId, bool isCorrect) =>
        new(taskId, "2+3", 5m)
        {
            CorrectAnswer = 5m,
            IsCorrect = isCorrect
        };
}
