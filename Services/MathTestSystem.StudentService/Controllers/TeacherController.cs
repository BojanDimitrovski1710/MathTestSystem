using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathTestSystem.StudentService.Controllers;

[ApiController]
[Route("api/teachers")]
[Authorize(Roles = "Teacher,Admin")]
[Tags("Teachers")]
public class TeacherController(
    ITeacherRepository teacherRepo,
    IStudentRepository studentRepo,
    IExamRepository examRepo,
    ILogger<TeacherController> logger) : ControllerBase
{
    [HttpGet("{teacherId}/students")]
    [ProducesResponseType<IReadOnlyList<StudentSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStudents(string teacherId)
    {
        logger.LogInformation("Student list requested for teacher {TeacherId}", teacherId);

        Teacher? teacher = await teacherRepo.GetByTeacherIdAsync(teacherId);

        if (teacher is null)
        {
            logger.LogWarning("Student list request failed — teacher {TeacherId} not found", teacherId);
            return NotFound(ResultCodes.TeacherNotFound);
        }

        IEnumerable<Student> students = await studentRepo.GetByTeacherUidAsync(teacher.Uid);

        List<StudentSummaryResponse> response = [];

        foreach (Student student in students)
        {
            IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);

            IReadOnlyList<ExamSummaryResponse> examSummaries = exams
                .Select(e => new ExamSummaryResponse(
                    e.Uid,
                    e.ExamId,
                    e.SubmittedAt,
                    e.Score,
                    e.Tasks.Count,
                    e.Tasks.Count(t => t.IsCorrect)))
                .ToList();

            response.Add(new StudentSummaryResponse(student.Uid, student.StudentId, examSummaries));
        }

        logger.LogInformation(
            "Returned {StudentCount} students for teacher {TeacherId}",
            response.Count, teacherId);

        return Ok(response);
    }
}
