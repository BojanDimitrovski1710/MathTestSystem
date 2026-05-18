using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Controllers;
using MathTestSystem.StudentService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MathTestSystem.StudentService.Tests.Controllers;

public class TeacherControllerTests
{
    private readonly ITeacherRepository _teacherRepo = Substitute.For<ITeacherRepository>();
    private readonly IStudentRepository _studentRepo = Substitute.For<IStudentRepository>();
    private readonly IExamRepository _examRepo = Substitute.For<IExamRepository>();
    private readonly TeacherController _controller;

    private static readonly Teacher DefaultTeacher = new("11111") { Id = 1 };

    public TeacherControllerTests()
    {
        _controller = new TeacherController(
            _teacherRepo,
            _studentRepo,
            _examRepo,
            Substitute.For<ILogger<TeacherController>>());
    }

    // -------------------------------------------------------------------------
    // GetStudents
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetStudents_TeacherNotFound_Returns404()
    {
        _teacherRepo.GetByTeacherIdAsync("99999").Returns((Teacher?)null);

        IActionResult result = await _controller.GetStudents("99999");

        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(ResultCodes.TeacherNotFound, notFound.Value);
    }

    [Fact]
    public async Task GetStudents_NoStudents_ReturnsEmptyList()
    {
        _teacherRepo.GetByTeacherIdAsync("11111").Returns(DefaultTeacher);
        _studentRepo.GetByTeacherUidAsync(DefaultTeacher.Uid).Returns([]);

        IActionResult result = await _controller.GetStudents("11111");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<StudentSummaryResponse> response = Assert.IsAssignableFrom<IReadOnlyList<StudentSummaryResponse>>(ok.Value);
        Assert.Empty(response);
    }

    [Fact]
    public async Task GetStudents_WithStudents_ReturnsAllStudentsWithExams()
    {
        Student student1 = new("12345", 1) { Id = 1 };
        Student student2 = new("54321", 1) { Id = 2 };

        _teacherRepo.GetByTeacherIdAsync("11111").Returns(DefaultTeacher);
        _studentRepo.GetByTeacherUidAsync(DefaultTeacher.Uid).Returns([student1, student2]);

        _examRepo.GetByStudentUidAsync(student1.Uid).Returns([BuildExam("1")]);
        _examRepo.GetByStudentUidAsync(student2.Uid).Returns([BuildExam("1"), BuildExam("2")]);

        IActionResult result = await _controller.GetStudents("11111");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<StudentSummaryResponse> response = Assert.IsAssignableFrom<IReadOnlyList<StudentSummaryResponse>>(ok.Value);
        Assert.Equal(2, response.Count);
        Assert.Single(response[0].Exams);
        Assert.Equal(2, response[1].Exams.Count);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Exam BuildExam(string examId) =>
        new(examId, 1)
        {
            Score = 100m,
            Tasks = [new("1", "2+3", 5m) { CorrectAnswer = 5m, IsCorrect = true }]
        };
}
