using Microsoft.EntityFrameworkCore;
using MathTestSystem.Domain.Constants;
using MathTestSystem.Domain.Entities;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.Infrastructure.Data;

namespace MathTestSystem.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByUidAsync(Guid uid)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Uid == uid);
    }

    public async Task<Student?> GetByStudentIdAsync(string studentId)
    {
        return await _context.Students
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public async Task<HashSet<string>> GetExistingIdsAsync(IEnumerable<string> ids)
    {
        HashSet<string> idSet = ids.ToHashSet();
        return [.. await _context.Students
            .Where(s => idSet.Contains(s.StudentId))
            .Select(s => s.StudentId)
            .ToListAsync()];
    }

    public async Task<IEnumerable<Student>> GetByStudentIdsAsync(IEnumerable<string> studentIds)
    {
        HashSet<string> idSet = studentIds.ToHashSet();
        return await _context.Students
            .Where(s => idSet.Contains(s.StudentId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByTeacherUidAsync(Guid teacherUid)
    {
        return await _context.Students
            .Include(s => s.Exams)
            .Where(s => s.Teacher.Uid == teacherUid)
            .ToListAsync();
    }

    public async Task<Student> AddAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task AddRangeAsync(IEnumerable<Student> students)
    {
        _context.Students.AddRange(students);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        bool exists = await _context.Students.AnyAsync(s => s.Uid == student.Uid);
        if (!exists)
            throw new InvalidOperationException(ResultCodes.StudentNotFound);

        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid uid)
    {
        Student? student = await _context.Students
            .FirstOrDefaultAsync(s => s.Uid == uid);

        if (student is null)
            throw new InvalidOperationException(ResultCodes.StudentNotFound);

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }
}
