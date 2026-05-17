using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathTestSystem.StudentService.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
[Tags("Students")]
public class StudentController(
    IStudentRepository studentRepo,
    IExamRepository examRepo) : ControllerBase
{
    [HttpGet("{studentId}/dashboard")]
    [ProducesResponseType<StudentDashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard(string studentId)
    {
        Student? student = await studentRepo.GetByStudentIdAsync(studentId);

        if (student is null)
            return NotFound(ResultCodes.StudentNotFound);

        IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);
        List<Exam> examList = exams.ToList();

        int overallCorrect = examList.Sum(e => e.Tasks.Count(t => t.IsCorrect));
        int overallTotal = examList.Sum(e => e.Tasks.Count);
        decimal overallScore = overallTotal > 0
            ? Math.Round((decimal)overallCorrect / overallTotal * 100, 2)
            : 0m;

        // In the current domain a student belongs to one teacher.
        // The grouping structure supports multiple teachers if that changes.
        TeacherDashboardEntry teacherEntry = new(
            student.Teacher.TeacherId,
            overallCorrect,
            overallTotal,
            overallScore,
            examList.Select(e =>
            {
                int correct = e.Tasks.Count(t => t.IsCorrect);
                int total = e.Tasks.Count;
                decimal score = total > 0
                    ? Math.Round((decimal)correct / total * 100, 2)
                    : 0m;

                return new ExamDashboardEntry(
                    e.Uid,
                    e.ExamId,
                    e.SubmittedAt,
                    score,
                    correct,
                    total,
                    e.Tasks
                        .Select(t => new TaskResponse(
                            t.TaskId,
                            t.Expression,
                            t.StudentAnswer,
                            t.CorrectAnswer,
                            t.IsCorrect,
                            t.ErrorMessage))
                        .ToList());
            }).ToList());

        StudentDashboardResponse response = new(
            student.StudentId,
            overallCorrect,
            overallTotal,
            overallScore,
            [teacherEntry]);

        return Ok(response);
    }

    [HttpGet("{studentUid:guid}/exams")]
    [ProducesResponseType<IReadOnlyList<ExamSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExams(Guid studentUid)
    {
        Student? student = await studentRepo.GetByUidAsync(studentUid);

        if (student is null)
            return NotFound(ResultCodes.StudentNotFound);

        IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);

        IReadOnlyList<ExamSummaryResponse> response = exams
            .Select(e => new ExamSummaryResponse(
                e.Uid,
                e.ExamId,
                e.SubmittedAt,
                e.Score,
                e.Tasks.Count,
                e.Tasks.Count(t => t.IsCorrect)))
            .ToList();

        return Ok(response);
    }

    [HttpGet("{studentUid:guid}/exams/{examUid:guid}")]
    [ProducesResponseType<ExamDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExamDetail(Guid studentUid, Guid examUid)
    {
        Student? student = await studentRepo.GetByUidAsync(studentUid);

        if (student is null)
            return NotFound(ResultCodes.StudentNotFound);

        Exam? exam = await examRepo.GetWithTasksAsync(examUid);

        if (exam is null || exam.Student.Uid != student.Uid)
            return NotFound(ResultCodes.ExamNotFound);

        ExamDetailResponse response = new(
            exam.Uid,
            exam.ExamId,
            exam.SubmittedAt,
            exam.Score,
            exam.Tasks
                .Select(t => new TaskResponse(
                    t.TaskId,
                    t.Expression,
                    t.StudentAnswer,
                    t.CorrectAnswer,
                    t.IsCorrect,
                    t.ErrorMessage))
                .ToList());

        return Ok(response);
    }
}
