using Microsoft.EntityFrameworkCore;
using MathTestSystem.Domain.Entities;

namespace MathTestSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamTask> ExamTasks => Set<ExamTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -------------------------------------------------------------------------
        // Teacher
        // -------------------------------------------------------------------------
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Uid)
                .IsRequired()
                .ValueGeneratedNever(); // We control generation via Guid.NewGuid() in the entity

            entity.HasIndex(t => t.Uid)
                .IsUnique();

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany(t => t.Students)
                .WithOne(s => s.Teacher)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------------------------
        // Student
        // -------------------------------------------------------------------------
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Uid)
                .IsRequired()
                .ValueGeneratedNever(); // We control generation via Guid.NewGuid() in the entity

            entity.HasIndex(s => s.Uid)
                .IsUnique();

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany(s => s.Exams)
                .WithOne(e => e.Student)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------------------------
        // Exam
        // -------------------------------------------------------------------------
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ExamId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.SubmittedAt)
                .IsRequired();

            entity.Property(e => e.Score)
                .HasPrecision(5, 2);

            entity.HasMany(e => e.Tasks)
                .WithOne(t => t.Exam)
                .HasForeignKey(t => t.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------------------------
        // ExamTask
        // -------------------------------------------------------------------------
        modelBuilder.Entity<ExamTask>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.TaskId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(t => t.Expression)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(t => t.StudentAnswer)
                .HasPrecision(18, 6);

            entity.Property(t => t.CorrectAnswer)
                .HasPrecision(18, 6);

            entity.Property(t => t.ErrorMessage)
                .HasMaxLength(500);
        });
    }
}
