using System.Windows.Input;

namespace MathTestSystem.StudentApp.Commands;

public class RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null) : ICommand
{
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        !_isExecuting && (canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        _isExecuting = true;
        CommandManager.InvalidateRequerySuggested();
        try { await executeAsync(); }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
