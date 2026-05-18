using System.Windows.Input;
using MathTestSystem.TeacherApp.Commands;

namespace MathTestSystem.TeacherApp.ViewModels;

public class HomeViewModel
{
    public HomeViewModel(AppViewModel app)
    {
        GradeTestCommand = new RelayCommand(app.NavigateToGrade);
        ViewStudentRecordsCommand = new RelayCommand(app.NavigateToStudentRecords);
    }

    public ICommand GradeTestCommand { get; }
    public ICommand ViewStudentRecordsCommand { get; }
}
