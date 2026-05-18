using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByUidAsync(Guid uid);
    Task<Student?> GetByStudentIdAsync(string studentId);
    Task<HashSet<string>> GetExistingIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<Student>> GetByStudentIdsAsync(IEnumerable<string> studentIds);
    Task<IEnumerable<Student>> GetByTeacherUidAsync(Guid teacherUid);
    Task<Student> AddAsync(Student student);
    Task AddRangeAsync(IEnumerable<Student> students);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Guid uid);
}
