using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.GradingService.Models;
using MathTestSystem.GradingService.Parsing;
using MathTestSystem.Infrastructure.Data;
using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MathTestSystem.GradingService.Services;

public class ExamGradingService : IGradingService
{
    private readonly IExamXmlParser _parser;
    private readonly IExpressionEvaluator _evaluator;
    private readonly ITeacherRepository _teacherRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IExamRepository _examRepo;
    private readonly UserManager<AppUser> _userManager;

    public ExamGradingService(
        IExamXmlParser parser,
        IExpressionEvaluator evaluator,
        ITeacherRepository teacherRepo,
        IStudentRepository studentRepo,
        IExamRepository examRepo,
        UserManager<AppUser> userManager)
    {
        _parser = parser;
        _evaluator = evaluator;
        _teacherRepo = teacherRepo;
        _studentRepo = studentRepo;
        _examRepo = examRepo;
        _userManager = userManager;
    }

    public async Task<GradeExamResponse> GradeAsync(string xml)
    {
        ParsedTeacherExam parsed = _parser.Parse(xml);

        string teacherId = parsed.TeacherId;
        List<string> studentIds = parsed.Students.Select(s => s.StudentId).ToList();


        HashSet<string> existingTeacherIds = await _teacherRepo.GetExistingIdsAsync([teacherId]);
        HashSet<string> existingStudentIds = await _studentRepo.GetExistingIdsAsync(studentIds);


        Dictionary<string, EvaluationResult> expressionCache = parsed.Students
            .SelectMany(s => s.Exams)
            .SelectMany(e => e.Tasks)
            .Select(t => t.Expression)
            .Distinct()
            .AsParallel()
            .Select(expr => (Expression: expr, Result: _evaluator.Evaluate(expr)))
            .ToDictionary(x => x.Expression, x => x.Result);

        Teacher teacher;

        if (existingTeacherIds.Contains(teacherId))
        {
            teacher = (await _teacherRepo.GetByTeacherIdAsync(teacherId))!;
        }
        else
        {
            teacher = await _teacherRepo.AddAsync(new Teacher { TeacherId = teacherId });
        }


        List<string> newStudentIds = studentIds.Where(id => !existingStudentIds.Contains(id)).ToList();

        // Bulk-check which Identity users already exist, then create missing ones in parallel
        HashSet<string> allIds = [teacherId, .. newStudentIds];
        HashSet<string> existingIdentityUsers = [.. await _userManager.Users
            .Where(u => allIds.Contains(u.UserName!))
            .Select(u => u.UserName!)
            .ToListAsync()];

        foreach (string id in allIds.Where(id => !existingIdentityUsers.Contains(id)))
            await EnsureIdentityUserAsync(id);

        List<Student> newStudents = newStudentIds
            .Select(id => new Student { StudentId = id, TeacherId = teacher.Id })
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
                    BuildExam(parsedExam, student.Id, expressionCache);

                examMappings.Add((exam, taskResults, parsedStudent.StudentId));
            }
        }


        await _examRepo.AddRangeAsync(examMappings.Select(x => x.Exam));


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

    private async Task EnsureIdentityUserAsync(string id)
    {
        if (await _userManager.FindByNameAsync(id) is not null)
            return;

        AppUser user = new() { UserName = id };
        IdentityResult result = await _userManager.CreateAsync(user, id);

        if (!result.Succeeded && result.Errors.Any(e => e.Code != "DuplicateUserName"))
            throw new InvalidOperationException(
                $"Failed to create Identity user for '{id}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    private static (Exam Exam, IReadOnlyList<TaskGradeResult> TaskResults) BuildExam(
        ParsedExam parsedExam,
        int studentFk,
        Dictionary<string, EvaluationResult> expressionCache)
    {
        List<ExamTask> tasks = [];
        List<TaskGradeResult> taskResults = [];

        foreach (ParsedTask parsedTask in parsedExam.Tasks)
        {
            EvaluationResult evalResult = expressionCache[parsedTask.Expression];

            ExamTask task = new()
            {
                TaskId = parsedTask.TaskId,
                Expression = parsedTask.Expression,
                StudentAnswer = parsedTask.StudentAnswer,
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

        Exam exam = new()
        {
            ExamId = parsedExam.ExamId,
            StudentId = studentFk,
            Score = score,
            Tasks = tasks
        };

        return (exam, taskResults);
    }
}
