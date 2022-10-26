using System.Windows;
using RemoteHealthcare.GUIs.Patient.ViewModels;

namespace RemoteHealthcare.GUIs.Patient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;

        public App()
        {
            _navigationStore = new NavigationStore();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore);
            
            MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(_navigationStore)
            };

            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}