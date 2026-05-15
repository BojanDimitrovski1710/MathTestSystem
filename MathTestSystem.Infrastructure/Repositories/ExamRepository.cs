using Microsoft.EntityFrameworkCore;
using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.Infrastructure.Data;

namespace MathTestSystem.Infrastructure.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly AppDbContext _context;

    public ExamRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Exam>> GetByStudentUidAsync(Guid studentUid)
    {
        return await _context.Exams
            .Include(e => e.Tasks)
            .Where(e => e.Student.Uid == studentUid)
            .ToListAsync();
    }

    public async Task<Exam?> GetWithTasksAsync(Guid uid)
    {
        return await _context.Exams
            .Include(e => e.Tasks)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Uid == uid);
    }

    public async Task<Exam> AddAsync(Exam exam)
    {
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();
        return exam;
    }

    public async Task UpdateAsync(Exam exam)
    {
        bool exists = await _context.Exams.AnyAsync(e => e.Uid == exam.Uid);
        if (!exists)
            throw new InvalidOperationException(ResultCodes.ExamNotFound);

        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
    }
}
