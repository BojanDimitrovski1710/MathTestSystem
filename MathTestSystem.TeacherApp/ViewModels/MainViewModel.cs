using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using MathTestSystem.TeacherApp.Commands;
using MathTestSystem.TeacherApp.Models;
using MathTestSystem.TeacherApp.Services;

namespace MathTestSystem.TeacherApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IGradingApiService _gradingService;

    private string _selectedFilePath = string.Empty;
    private string _statusMessage = "Ready.";
    private bool _isSubmitting;

    public MainViewModel() : this(new GradingApiService()) { }

    public MainViewModel(IGradingApiService gradingService)
    {
        _gradingService = gradingService;
        BrowseCommand = new RelayCommand(Browse);
        SubmitCommand = new RelayCommand(async () => await SubmitAsync(), CanSubmit);
    }

    public ICommand BrowseCommand { get; }
    public ICommand SubmitCommand { get; }

    public ObservableCollection<StudentResultViewModel> Students { get; } = [];

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set { _selectedFilePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(SubmitButtonText)); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsSubmitting
    {
        get => _isSubmitting;
        set { _isSubmitting = value; OnPropertyChanged(); OnPropertyChanged(nameof(SubmitButtonText)); }
    }

    public string SubmitButtonText => IsSubmitting ? "Grading..." : "Submit";

    private void Browse()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Select XML Exam File",
            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
            SelectedFilePath = dialog.FileName;
    }

    private bool CanSubmit() => !IsSubmitting && !string.IsNullOrWhiteSpace(SelectedFilePath);

    private async Task SubmitAsync()
    {
        IsSubmitting = true;
        StatusMessage = "Submitting...";
        Students.Clear();

        try
        {
            string xml = await File.ReadAllTextAsync(SelectedFilePath);
            GradeExamResponse response = await _gradingService.GradeAsync(xml);

            foreach (StudentGradeResult student in response.Students)
                Students.Add(new StudentResultViewModel(student));

            StatusMessage = $"Graded successfully. Teacher: {response.TeacherId} — {response.Students.Count} student(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// -------------------------------------------------------------------------
// Child view models
// -------------------------------------------------------------------------

public class StudentResultViewModel(StudentGradeResult model)
{
    public string StudentId => model.StudentId;
    public Guid StudentUid => model.StudentUid;
    public IReadOnlyList<ExamResultViewModel> Exams { get; } =
        model.Exams.Select(e => new ExamResultViewModel(e)).ToList();

    public override string ToString() => $"Student: {model.StudentId}";
}

public class ExamResultViewModel(ExamGradeResult model)
{
    public string ExamId => model.ExamId;
    public decimal Score => model.Score;
    public IReadOnlyList<TaskResultViewModel> Tasks { get; } =
        model.Tasks.Select(t => new TaskResultViewModel(t)).ToList();

    public override string ToString() => $"Exam {model.ExamId}  —  Score: {model.Score:F1}%";
}

public class TaskResultViewModel(TaskGradeResult model)
{
    public string TaskId => model.TaskId;
    public string Expression => model.Expression;
    public decimal StudentAnswer => model.StudentAnswer;
    public decimal? CorrectAnswer => model.CorrectAnswer;
    public bool IsCorrect => model.IsCorrect;
    public bool HasError => model.HasError;

    public string StatusIcon => model.HasError ? "⚠" : model.IsCorrect ? "✓" : "✗";
    public string StatusColor => model.HasError ? "Orange" : model.IsCorrect ? "Green" : "Red";

    public string CorrectAnswerText => model.HasError
        ? $"[{model.ErrorCode}]"
        : model.IsCorrect
            ? string.Empty
            : $"(correct: {model.CorrectAnswer})";

    public override string ToString() =>
        $"{StatusIcon}  {model.Expression} = {model.StudentAnswer}  {CorrectAnswerText}";
}
