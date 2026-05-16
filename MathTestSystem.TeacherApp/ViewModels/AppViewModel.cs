using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MathTestSystem.TeacherApp.Commands;
using MathTestSystem.TeacherApp.Services;

namespace MathTestSystem.TeacherApp.ViewModels;

public class AppViewModel : INotifyPropertyChanged
{
    private object _currentView = null!;

    public AppViewModel()
    {
        GoHomeCommand = new RelayCommand(GoHome);
        NavigateHome();
    }

    public ICommand GoHomeCommand { get; }

    public object CurrentView
    {
        get => _currentView;
        private set { _currentView = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowBackButton)); }
    }

    public bool ShowBackButton => CurrentView is not HomeViewModel;

    private void NavigateHome() => CurrentView = new HomeViewModel(this);
    private void GoHome() => NavigateHome();

    public void NavigateToGrade() =>
        CurrentView = new GradeViewModel(new GradingApiService());

    public void NavigateToStudentRecords() =>
        CurrentView = new StudentRecordsViewModel(new StudentApiService());

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
