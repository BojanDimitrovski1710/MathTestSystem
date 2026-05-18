using System.Windows;
using MathTestSystem.TeacherApp.ViewModels;

namespace MathTestSystem.TeacherApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new AppViewModel();
    }
}
