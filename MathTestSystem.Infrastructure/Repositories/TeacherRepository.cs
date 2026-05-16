using Microsoft.EntityFrameworkCore;
using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.Infrastructure.Data;

namespace MathTestSystem.Infrastructure.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher?> GetByUidAsync(Guid uid)
    {
        return await _context.Teachers
            .FirstOrDefaultAsync(t => t.Uid == uid);
    }

    public async Task<Teacher?> GetByTeacherIdAsync(string teacherId)
    {
        return await _context.Teachers
            .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
    }

    public async Task<IEnumerable<Teacher>> GetAllAsync()
    {
        return await _context.Teachers
            .Include(t => t.Students)
            .ToListAsync();
    }

    public async Task<Teacher> AddAsync(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        bool exists = await _context.Teachers.AnyAsync(t => t.Uid == teacher.Uid);
        if (!exists)
            throw new InvalidOperationException(ResultCodes.TeacherNotFound);

        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid uid)
    {
        Teacher? teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Uid == uid);

        if (teacher is null)
            throw new InvalidOperationException(ResultCodes.TeacherNotFound);

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
    }
}
