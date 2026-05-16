using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface ITeacherRepository
{
    Task<Teacher?> GetByUidAsync(Guid uid);
    Task<Teacher?> GetByTeacherIdAsync(string teacherId);
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<Teacher> AddAsync(Teacher teacher);
    Task UpdateAsync(Teacher teacher);
    Task DeleteAsync(Guid uid);
}
