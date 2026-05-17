using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface IExamRepository
{
    Task<IEnumerable<Exam>> GetByStudentUidAsync(Guid studentUid);
    Task<Exam?> GetWithTasksAsync(Guid uid);
    Task<Exam> AddAsync(Exam exam);
    Task AddRangeAsync(IEnumerable<Exam> exams);
    Task UpdateAsync(Exam exam);
}
