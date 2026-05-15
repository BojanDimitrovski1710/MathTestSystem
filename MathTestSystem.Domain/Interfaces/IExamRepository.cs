using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Domain.Interfaces;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(int id);
    Task<IEnumerable<Exam>> GetByStudentIdAsync(string studentId);
    Task<Exam?> GetWithTasksAsync(int id);
    Task<Exam> AddAsync(Exam exam);
    Task UpdateAsync(Exam exam);
}
