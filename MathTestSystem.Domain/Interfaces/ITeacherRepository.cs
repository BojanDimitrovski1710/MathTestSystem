using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface ITeacherRepository
{
    Task<Teacher?> GetByIdAsync(int id);
    Task<Teacher?> GetByTeacherIdAsync(string teacherId);
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<Teacher> AddAsync(Teacher teacher);
    Task UpdateAsync(Teacher teacher);
    Task DeleteAsync(int id);
}
