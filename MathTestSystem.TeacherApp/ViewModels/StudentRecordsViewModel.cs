using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MathTestSystem.TeacherApp.Commands;
using MathTestSystem.TeacherApp.Models;
using MathTestSystem.TeacherApp.Services;
using MathTestSystem.TeacherApp.State;

namespace MathTestSystem.TeacherApp.ViewModels;

public class StudentRecordsViewModel : INotifyPropertyChanged
{
    private readonly IStudentApiService _studentService;
    private string _teacherId = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoading;
    private bool _hasLoaded;

    public bool IsAdminMode { get; }

    public StudentRecordsViewModel(IStudentApiService studentService, AuthState authState)
    {
        _studentService = studentService;
        IsAdminMode = authState.IsAdmin;
        LoadCommand = new RelayCommand(async () => await LoadAsync(), CanLoad);

        if (!IsAdminMode)
        {
            // Pre-fill with the logged-in teacher's ID and auto-load
            _teacherId = authState.Username ?? string.Empty;
            _statusMessage = "Loading your students…";
            _ = LoadAsync();
        }
        else
        {
            _statusMessage = "Enter a Teacher ID and press Load.";
        }
    }

    public ICommand LoadCommand { get; }
    public ObservableCollection<StudentOverviewViewModel> Students { get; } = [];

    public string TeacherId
    {
        get => _teacherId;
        set { _teacherId = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool HasLoaded
    {
        get => _hasLoaded;
        set { _hasLoaded = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowEmptyState)); }
    }

    public bool ShowEmptyState => HasLoaded && Students.Count == 0;

    private bool CanLoad() => !IsLoading && !string.IsNullOrWhiteSpace(TeacherId);

    private async Task LoadAsync()
    {
        IsLoading = true;
        HasLoaded = false;
        StatusMessage = "Loading...";
        Students.Clear();

        try
        {
            List<StudentSummaryResponse> students = await _studentService.GetTeacherStudentsAsync(TeacherId.Trim());

            foreach (StudentSummaryResponse student in students)
                Students.Add(new StudentOverviewViewModel(student));

            HasLoaded = true;
            StatusMessage = Students.Count > 0
                ? $"Loaded {Students.Count} student(s) for teacher '{TeacherId.Trim()}'."
                : $"No students found for teacher '{TeacherId.Trim()}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
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

public class StudentOverviewViewModel(StudentSummaryResponse model)
{
    public string StudentId => model.StudentId;
    public Guid StudentUid => model.StudentUid;
    public int ExamCount => model.Exams.Count;
    public decimal AverageScore => model.Exams.Count > 0
        ? Math.Round(model.Exams.Average(e => e.Score), 1)
        : 0m;
    public IReadOnlyList<ExamSummaryResponse> Exams => model.Exams;
}
