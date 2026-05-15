using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByUidAsync(Guid uid);
    Task<IEnumerable<Student>> GetByTeacherUidAsync(Guid teacherUid);
    Task<Student> AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Guid uid);
}
