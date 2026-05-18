using System.Diagnostics;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.GradingService.Models;
using MathTestSystem.GradingService.Parsing;
using MathTestSystem.Infrastructure.Data;
using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MathTestSystem.GradingService.Services;

public class ExamGradingService : IGradingService
{
    private readonly IExamXmlParser _parser;
    private readonly IExpressionEvaluator _evaluator;
    private readonly ITeacherRepository _teacherRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IExamRepository _examRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<ExamGradingService> _logger;

    public ExamGradingService(
        IExamXmlParser parser,
        IExpressionEvaluator evaluator,
        ITeacherRepository teacherRepo,
        IStudentRepository studentRepo,
        IExamRepository examRepo,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<ExamGradingService> logger)
    {
        _parser = parser;
        _evaluator = evaluator;
        _teacherRepo = teacherRepo;
        _studentRepo = studentRepo;
        _examRepo = examRepo;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<GradeExamResponse> GradeAsync(string xml)
    {
        _logger.LogInformation("Grading request received — parsing XML");
        Stopwatch sw = Stopwatch.StartNew();

        ParsedTeacherExam parsed = _parser.Parse(xml);

        string teacherId = parsed.TeacherId;
        List<string> studentIds = parsed.Students.Select(s => s.StudentId).Distinct().ToList();
        int totalExams = parsed.Students.Sum(s => s.Exams.Count);
        int totalTasks = parsed.Students.SelectMany(s => s.Exams).Sum(e => e.Tasks.Count);

        _logger.LogInformation(
            "Parsed XML for teacher {TeacherId}: {StudentCount} students, {ExamCount} exams, {TaskCount} tasks",
            teacherId, studentIds.Count, totalExams, totalTasks);

        HashSet<string> existingTeacherIds = await _teacherRepo.GetExistingIdsAsync([teacherId]);
        HashSet<string> existingStudentIds = await _studentRepo.GetExistingIdsAsync(studentIds);

        int uniqueExpressions = parsed.Students
            .SelectMany(s => s.Exams).SelectMany(e => e.Tasks)
            .Select(t => t.Expression).Distinct().Count();

        Dictionary<string, EvaluationResult> expressionCache = parsed.Students
            .SelectMany(s => s.Exams)
            .SelectMany(e => e.Tasks)
            .Select(t => t.Expression)
            .Distinct()
            .AsParallel()
            .Select(expr => (Expression: expr, Result: _evaluator.Evaluate(expr)))
            .ToDictionary(x => x.Expression, x => x.Result);

        _logger.LogInformation(
            "Evaluated {UniqueCount} unique expressions (cache covers {TaskCount} total tasks)",
            uniqueExpressions, totalTasks);

        Teacher teacher;

        if (existingTeacherIds.Contains(teacherId))
        {
            teacher = (await _teacherRepo.GetByTeacherIdAsync(teacherId))!;
            _logger.LogDebug("Teacher {TeacherId} already exists", teacherId);
        }
        else
        {
            _logger.LogInformation("Creating new teacher {TeacherId}", teacherId);
            teacher = await _teacherRepo.AddAsync(new Teacher(teacherId));
        }

        List<string> newStudentIds = studentIds.Where(id => !existingStudentIds.Contains(id)).ToList();

        if (newStudentIds.Count > 0)
            _logger.LogInformation(
                "{NewCount} new students out of {TotalCount} — creating identity users and records",
                newStudentIds.Count, studentIds.Count);

        // Bulk-check which Identity users already exist, then create missing ones sequentially
        HashSet<string> allIds = [teacherId, .. newStudentIds];
        HashSet<string> existingIdentityUsers = [.. _userManager.Users
            .Where(u => allIds.Contains(u.UserName!))
            .Select(u => u.UserName!)
            .ToList()];

        // Create teacher identity user if missing
        if (!existingIdentityUsers.Contains(teacherId))
            await EnsureIdentityUserAsync(teacherId, isStudent: false);

        // Create student identity users if missing
        foreach (string studentId in newStudentIds.Where(id => !existingIdentityUsers.Contains(id)))
            await EnsureIdentityUserAsync(studentId, isStudent: true);

        List<Student> newStudents = newStudentIds
            .Select(id => new Student(id, teacher.Id))
            .ToList();

        if (newStudents.Count > 0)
            await _studentRepo.AddRangeAsync(newStudents);

        // Fetch full entities for existing students (need their PKs for exam FKs)
        Dictionary<string, Student> studentMap = (await _studentRepo.GetByStudentIdsAsync(
                studentIds.Where(existingStudentIds.Contains)))
            .ToDictionary(s => s.StudentId);

        // Merge newly saved students into the map (PKs populated by EF Core after AddRangeAsync)
        foreach (Student s in newStudents)
            studentMap[s.StudentId] = s;

        List<(Exam Exam, IReadOnlyList<TaskGradeResult> TaskResults, string StudentId)> examMappings = [];

        foreach (ParsedStudent parsedStudent in parsed.Students)
        {
            Student student = studentMap[parsedStudent.StudentId];

            foreach (ParsedExam parsedExam in parsedStudent.Exams)
            {
                (Exam exam, IReadOnlyList<TaskGradeResult> taskResults) =
                    BuildExam(parsedExam, student.Id, teacher.Id, expressionCache);

                examMappings.Add((exam, taskResults, parsedStudent.StudentId));
            }
        }

        await _examRepo.AddRangeAsync(examMappings.Select(x => x.Exam));

        sw.Stop();
        _logger.LogInformation(
            "Grading complete for teacher {TeacherId} — {ExamCount} exams saved in {ElapsedMs}ms",
            teacherId, examMappings.Count, sw.ElapsedMilliseconds);

        List<StudentGradeResult> studentResults = parsed.Students
            .Select(parsedStudent =>
            {
                Student student = studentMap[parsedStudent.StudentId];

                List<ExamGradeResult> examResults = examMappings
                    .Where(x => x.StudentId == parsedStudent.StudentId)
                    .Select(x => new ExamGradeResult(x.Exam.Uid, x.Exam.ExamId, x.Exam.Score, x.TaskResults))
                    .ToList();

                return new StudentGradeResult(student.Uid, parsedStudent.StudentId, examResults);
            })
            .ToList();

        return new GradeExamResponse(parsed.TeacherId, studentResults);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task EnsureIdentityUserAsync(string id, bool isStudent = false)
    {
        if (await _userManager.FindByNameAsync(id) is not null)
            return;

        AppUser user = new() { UserName = id };
        IdentityResult result = await _userManager.CreateAsync(user, id);

        if (!result.Succeeded && result.Errors.Any(e => e.Code != "DuplicateUserName"))
        {
            string errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create Identity user for '{UserId}': {Errors}", id, errors);
            throw new InvalidOperationException($"Failed to create Identity user for '{id}': {errors}");
        }

        // Ensure role exists
        string role = isStudent ? "Student" : "Teacher";
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // Assign role to user
        await _userManager.AddToRoleAsync(user, role);
    }

    private static (Exam Exam, IReadOnlyList<TaskGradeResult> TaskResults) BuildExam(
        ParsedExam parsedExam,
        int studentFk,
        int teacherFk,
        Dictionary<string, EvaluationResult> expressionCache)
    {
        List<ExamTask> tasks = [];
        List<TaskGradeResult> taskResults = [];

        foreach (ParsedTask parsedTask in parsedExam.Tasks)
        {
            EvaluationResult evalResult = expressionCache[parsedTask.Expression];

            ExamTask task = new(parsedTask.TaskId, parsedTask.Expression, parsedTask.StudentAnswer)
            {
                CorrectAnswer = evalResult.Success ? evalResult.Value : null,
                IsCorrect = evalResult.Success && evalResult.Value == parsedTask.StudentAnswer,
                ErrorMessage = evalResult.Success ? null : evalResult.ErrorCode
            };

            tasks.Add(task);
            taskResults.Add(new TaskGradeResult(
                task.TaskId,
                task.Expression,
                task.StudentAnswer,
                task.CorrectAnswer,
                task.IsCorrect,
                task.ErrorMessage));
        }

        int correctCount = tasks.Count(t => t.IsCorrect);
        int gradableCount = tasks.Count(t => t.ErrorMessage is null);
        decimal score = gradableCount > 0
            ? Math.Round((decimal)correctCount / gradableCount * 100, 2)
            : 0m;

        Exam exam = new(parsedExam.ExamId, studentFk, teacherFk)
        {
            Score = score,
            Tasks = tasks
        };

        return (exam, taskResults);
    }
}
