using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.StudentService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathTestSystem.StudentService.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = "Student,Admin")]
[Tags("Students")]
public class StudentController(
    IStudentRepository studentRepo,
    IExamRepository examRepo,
    ILogger<StudentController> logger) : ControllerBase
{
    [HttpGet("{studentId}/dashboard")]
    [ProducesResponseType<StudentDashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(string studentId)
    {
        logger.LogInformation("Dashboard requested for student {StudentId}", studentId);

        Student? student = await studentRepo.GetByStudentIdAsync(studentId);

        if (student is null)
        {
            logger.LogWarning("Dashboard request failed — student {StudentId} not found", studentId);
            return NotFound(ResultCodes.StudentNotFound);
        }

        IEnumerable<Exam> exams = await examRepo.GetByStudentUidAsync(student.Uid);
        List<Exam> examList = exams.ToList();

        int overallCorrect = examList.Sum(e => e.Tasks.Count(t => t.IsCorrect));
        int overallTotal = examList.Sum(e => e.Tasks.Count);
        decimal overallScore = overallTotal > 0
            ? Math.Round((decimal)overallCorrect / overallTotal * 100, 2)
            : 0m;

        // Group exams by the teacher who uploaded them — a student can appear in
        // XMLs from multiple teachers, so each teacher gets its own dashboard entry.
        List<TeacherDashboardEntry> teacherEntries = examList
            .GroupBy(e => e.UploadedByTeacher!.TeacherId)
            .Select(group =>
            {
                int groupCorrect = group.Sum(e => e.Tasks.Count(t => t.IsCorrect));
                int groupTotal = group.Sum(e => e.Tasks.Count);
                decimal groupScore = groupTotal > 0
                    ? Math.Round((decimal)groupCorrect / groupTotal * 100, 2)
                    : 0m;

                List<ExamDashboardEntry> examEntries = group.Select(e =>
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
                }).ToList();

                return new TeacherDashboardEntry(group.Key, groupCorrect, groupTotal, groupScore, examEntries);
            })
            .ToList();

        StudentDashboardResponse response = new(
            student.StudentId,
            overallCorrect,
            overallTotal,
            overallScore,
            teacherEntries);

        return Ok(response);
    }

    [HttpGet("{studentUid:guid}/exams")]
    [ProducesResponseType<IReadOnlyList<ExamSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExams(Guid studentUid)
    {
        logger.LogInformation("Exam list requested for student {StudentUid}", studentUid);

        Student? student = await studentRepo.GetByUidAsync(studentUid);

        if (student is null)
        {
            logger.LogWarning("Exam list request failed — student {StudentUid} not found", studentUid);
            return NotFound(ResultCodes.StudentNotFound);
        }

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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExamDetail(Guid studentUid, Guid examUid)
    {
        logger.LogInformation("Exam detail requested for student {StudentUid}, exam {ExamUid}", studentUid, examUid);

        Student? student = await studentRepo.GetByUidAsync(studentUid);

        if (student is null)
        {
            logger.LogWarning("Exam detail request failed — student {StudentUid} not found", studentUid);
            return NotFound(ResultCodes.StudentNotFound);
        }

        Exam? exam = await examRepo.GetWithTasksAsync(examUid);

        if (exam is null || exam.Student!.Uid != student.Uid)
        {
            logger.LogWarning("Exam detail request failed — exam {ExamUid} not found for student {StudentUid}", examUid, studentUid);
            return NotFound(ResultCodes.ExamNotFound);
        }

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
