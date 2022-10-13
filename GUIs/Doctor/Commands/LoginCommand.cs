using System;
using System.Threading;
using System.Threading.Tasks;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class LoginCommand : BaseCommand
{
    private readonly NavigationStore _navigationStore;
    private readonly LoginWindowViewModel _loginWindowViewModel;

    public LoginCommand(LoginWindowViewModel viewModel, NavigationStore navigationStore)
    {
        _loginWindowViewModel = viewModel;
        _navigationStore = navigationStore;
    }

    public override void Execute(object? parameter)
    { 
        ExecuteAsync();
    }

    public override async Task ExecuteAsync()
    {
        Console.WriteLine("Executing async command");

        //Window windowToClose = window as Window;
        await _loginWindowViewModel._client._client.ConnectAsync("127.0.0.1", 15243);

        if (!_loginWindowViewModel._client.loggedIn)
        {
            _loginWindowViewModel._client.username = _loginWindowViewModel.Username;
            _loginWindowViewModel._client.password = _loginWindowViewModel.SecureStringToString(_loginWindowViewModel.SecurePassword);
            try
            {
                new Thread(async () =>
                {
                    await _loginWindowViewModel._client.AskForLoginAsync();
                }).Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            await Task.Delay(1000);
            
            if (_loginWindowViewModel._client.loggedIn)
            {
                _navigationStore.CurrentViewModel = new DoctorViewModel();
                //_client.RequestClients();
                /*wait _client.RequestPatientDataAsync();
                DoctorViewModel doctorViewModel = new DoctorViewModel();
                DoctorView doctorView = new DoctorView
                {
                    DataContext = doctorViewModel
                };*/
                // windowToClose.Close();
                // doctorView.Show();
            }
        }
    }
}