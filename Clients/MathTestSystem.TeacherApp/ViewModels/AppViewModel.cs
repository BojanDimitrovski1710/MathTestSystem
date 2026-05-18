using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MathTestSystem.TeacherApp.Commands;
using MathTestSystem.TeacherApp.Services;
using MathTestSystem.TeacherApp.State;

namespace MathTestSystem.TeacherApp.ViewModels;

public class AppViewModel : INotifyPropertyChanged
{
    private readonly AuthState _authState = new();
    private object _currentView = null!;

    public AppViewModel()
    {
        GoHomeCommand = new RelayCommand(GoHome);
        LogoutCommand = new RelayCommand(Logout);
        NavigateToLogin();
    }

    public ICommand GoHomeCommand { get; }
    public ICommand LogoutCommand { get; }

    public object CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(IsLoggedIn));
        }
    }

    public bool ShowBackButton => CurrentView is not HomeViewModel and not LoginViewModel;
    public bool IsLoggedIn => CurrentView is not LoginViewModel;

    private void NavigateToLogin() =>
        CurrentView = new LoginViewModel(new AuthApiService(), _authState, this);

    private void GoHome() => NavigateHome();

    private void Logout()
    {
        _authState.Clear();
        NavigateToLogin();
    }

    public void NavigateHome() => CurrentView = new HomeViewModel(this);

    public void NavigateToGrade() =>
        CurrentView = new GradeViewModel(new GradingApiService(_authState));

    public void NavigateToStudentRecords() =>
        CurrentView = new StudentRecordsViewModel(new StudentApiService(_authState), _authState);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
