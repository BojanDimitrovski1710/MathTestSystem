using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathTestSystem.StudentApp.Models;
using MathTestSystem.StudentApp.Services;
using MathTestSystem.StudentApp.State;

namespace MathTestSystem.StudentApp.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly IStudentApiService _studentService;
    private readonly string _studentId;

    private string _statusMessage = "Loading…";
    private bool _isLoading = true;
    private int _overallCorrect;
    private int _overallTotal;
    private decimal _overallScore;

    public DashboardViewModel(IStudentApiService studentService, AuthState authState)
    {
        _studentService = studentService;
        _studentId = authState.Username ?? string.Empty;
        _ = LoadAsync();
    }

    public ObservableCollection<TeacherEntryViewModel> Teachers { get; } = [];

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatusMessage)); }
    }

    public bool HasStatusMessage => !string.IsNullOrEmpty(_statusMessage);

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public int OverallCorrect
    {
        get => _overallCorrect;
        private set { _overallCorrect = value; OnPropertyChanged(); }
    }

    public int OverallTotal
    {
        get => _overallTotal;
        private set { _overallTotal = value; OnPropertyChanged(); }
    }

    public decimal OverallScore
    {
        get => _overallScore;
        private set { _overallScore = value; OnPropertyChanged(); }
    }

    private async Task LoadAsync()
    {
        try
        {
            StudentDashboardResponse dashboard = await _studentService.GetDashboardAsync(_studentId);

            OverallCorrect = dashboard.OverallCorrect;
            OverallTotal = dashboard.OverallTotal;
            OverallScore = dashboard.OverallScore;

            foreach (TeacherDashboardEntry teacher in dashboard.Teachers)
                Teachers.Add(new TeacherEntryViewModel(teacher));

            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dashboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class TeacherEntryViewModel(TeacherDashboardEntry teacher)
{
    public string TeacherId => teacher.TeacherId;
    public int CorrectTasks => teacher.CorrectTasks;
    public int TotalTasks => teacher.TotalTasks;
    public decimal Score => teacher.Score;
    public string ScoreSummary => $"{CorrectTasks}/{TotalTasks} ({Score:F1}%)";
    public IReadOnlyList<ExamEntryViewModel> Exams { get; } =
        teacher.Exams.Select(e => new ExamEntryViewModel(e)).ToList();
}

public class ExamEntryViewModel(ExamDashboardEntry exam)
{
    public string ExamId => exam.ExamId;
    public DateTime SubmittedAt => exam.SubmittedAt;
    public decimal Score => exam.Score;
    public int CorrectTasks => exam.CorrectTasks;
    public int TotalTasks => exam.TotalTasks;
    public string ScoreSummary => $"{CorrectTasks}/{TotalTasks} ({Score:F1}%)";
    public IReadOnlyList<TaskEntry> Tasks => exam.Tasks;
}
