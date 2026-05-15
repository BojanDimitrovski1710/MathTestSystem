using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByStudentIdAsync(string studentId);
    Task<IEnumerable<Student>> GetByTeacherIdAsync(string teacherId);
    Task<Student> AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(int id);
}
