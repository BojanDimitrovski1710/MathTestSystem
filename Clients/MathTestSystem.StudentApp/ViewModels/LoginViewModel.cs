using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MathTestSystem.StudentApp.Commands;
using MathTestSystem.StudentApp.Services;
using MathTestSystem.StudentApp.State;

namespace MathTestSystem.StudentApp.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthApiService _authService;
    private readonly AuthState _authState;
    private readonly AppViewModel _app;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(IAuthApiService authService, AuthState authState, AppViewModel app)
    {
        _authService = authService;
        _authState = authState;
        _app = app;
        LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
    }

    public ICommand LoginCommand { get; }

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatusMessage)); }
    }

    public bool HasStatusMessage => !string.IsNullOrEmpty(_statusMessage);

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    private bool CanLogin() => !IsLoading
        && !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password);

    private async Task LoginAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            (string token, string role) = await _authService.LoginAsync(Username.Trim(), Password);
            _authState.SetToken(token, Username.Trim(), role);
            _app.NavigateToDashboard();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
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
