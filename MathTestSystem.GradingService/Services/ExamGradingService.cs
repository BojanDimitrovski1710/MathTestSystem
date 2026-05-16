using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.GradingService.Models;
using MathTestSystem.GradingService.Parsing;
using MathTestSystem.Infrastructure.Data;
using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Models;
using Microsoft.AspNetCore.Identity;

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

        Teacher teacher = await GetOrCreateTeacherAsync(parsed.TeacherId);

        List<StudentGradeResult> studentResults = [];

        Dictionary<string, EvaluationResult> expressionCache = [];

        foreach (ParsedStudent parsedStudent in parsed.Students)
        {
            Student student = await GetOrCreateStudentAsync(parsedStudent.StudentId, teacher.Id);

            List<ExamGradeResult> examResults = [];

            foreach (ParsedExam parsedExam in parsedStudent.Exams)
            {
                ExamGradeResult examResult = await GradeExamAsync(parsedExam, student.Id, expressionCache);
                examResults.Add(examResult);
            }

            studentResults.Add(new StudentGradeResult(student.Uid, parsedStudent.StudentId, examResults));
        }

        return new GradeExamResponse(parsed.TeacherId, studentResults);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<Teacher> GetOrCreateTeacherAsync(string teacherId)
    {
        Teacher? teacher = await _teacherRepo.GetByTeacherIdAsync(teacherId);

        if (teacher is not null)
            return teacher;

        await EnsureIdentityUserAsync(teacherId);
        return await _teacherRepo.AddAsync(new Teacher { TeacherId = teacherId });
    }

    private async Task<Student> GetOrCreateStudentAsync(string studentId, int teacherFk)
    {
        Student? student = await _studentRepo.GetByStudentIdAsync(studentId);

        if (student is not null)
            return student;

        await EnsureIdentityUserAsync(studentId);
        return await _studentRepo.AddAsync(new Student
        {
            StudentId = studentId,
            TeacherId = teacherFk
        });
    }

    /// <summary>
    /// Creates an Identity user for the given ID if one does not already exist.
    /// The initial password equals the ID — spoofed auth for demo purposes.
    /// </summary>
    private async Task EnsureIdentityUserAsync(string id)
    {
        if (await _userManager.FindByNameAsync(id) is not null)
            return;

        AppUser user = new() { UserName = id };
        await _userManager.CreateAsync(user, id);
    }

    private async Task<ExamGradeResult> GradeExamAsync(
        ParsedExam parsedExam,
        int studentFk,
        Dictionary<string, EvaluationResult> expressionCache)
    {
        List<ExamTask> tasks = [];
        List<TaskGradeResult> taskResults = [];

        foreach (ParsedTask parsedTask in parsedExam.Tasks)
        {
            if (!expressionCache.TryGetValue(parsedTask.Expression, out EvaluationResult evalResult))
            {
                evalResult = _evaluator.Evaluate(parsedTask.Expression);
                expressionCache[parsedTask.Expression] = evalResult;
            }

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

        Exam saved = await _examRepo.AddAsync(exam);

        return new ExamGradeResult(saved.Uid, parsedExam.ExamId, score, taskResults);
    }
}
