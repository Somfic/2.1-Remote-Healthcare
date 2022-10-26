using System.Windows;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var navigationStore = new NavigationStore();

        navigationStore.CurrentViewModel = new LoginWindowViewModel(navigationStore);

        MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(navigationStore)
        };
        MainWindow.Show();

        base.OnStartup(e);
    }
}