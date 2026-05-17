using System.Windows;
using System.Windows.Controls;
using MathTestSystem.StudentApp.ViewModels;

namespace MathTestSystem.StudentApp.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => PasswordBox.PasswordChanged += OnPasswordChanged;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }
}
