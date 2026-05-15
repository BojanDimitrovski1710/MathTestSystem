using Microsoft.EntityFrameworkCore;
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

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students.FindAsync(id);
    }

    public async Task<Student?> GetByUidAsync(Guid uid)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Uid == uid);
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

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Student? student = await _context.Students.FindAsync(id);
        if (student is not null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}
