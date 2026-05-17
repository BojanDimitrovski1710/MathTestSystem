using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MathTestSystem.StudentApp.Commands;
using MathTestSystem.StudentApp.Services;
using MathTestSystem.StudentApp.State;

namespace MathTestSystem.StudentApp.ViewModels;

public class AppViewModel : INotifyPropertyChanged
{
    private readonly AuthState _authState = new();
    private object _currentView = null!;

    public AppViewModel()
    {
        LogoutCommand = new RelayCommand(async () => await Task.Run(Logout));
        NavigateToLogin();
    }

    public ICommand LogoutCommand { get; }

    public object CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLoggedIn));
        }
    }

    public bool IsLoggedIn => CurrentView is not LoginViewModel;

    private void NavigateToLogin() =>
        CurrentView = new LoginViewModel(new AuthApiService(), _authState, this);

    private void Logout()
    {
        _authState.Clear();
        NavigateToLogin();
    }

    public void NavigateToDashboard() =>
        CurrentView = new DashboardViewModel(new StudentApiService(_authState), _authState);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
