using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MathTestSystem.Domain.Interfaces;
using MathTestSystem.Infrastructure.Data;
using MathTestSystem.Infrastructure.Repositories;

namespace MathTestSystem.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));

        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();

        return services;
    }
}
