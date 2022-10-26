using System;
using System.Threading;
using System.Threading.Tasks;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class LoginCommand : BaseCommand
{
    private readonly NavigationStore _navigationStore;
    private readonly LoginWindowViewModel _loginWindowViewModel;
    private readonly NavigationService<DoctorViewModel> _navigationService;

    public LoginCommand(LoginWindowViewModel viewModel, NavigationService<DoctorViewModel> navigationService)
    {
        _loginWindowViewModel = viewModel;
        _navigationService = navigationService;
    }

    public override void Execute(object? parameter)
    { 
        ExecuteAsync();
    }

    /// <summary>
    /// It connects to the server, sends the login credentials, waits for the server to respond, and then navigates to the
    /// next window
    /// </summary>
    public override async Task ExecuteAsync()
    {
        await _loginWindowViewModel.DoctorClient.Client.ConnectAsync("127.0.0.1", 15243);

        if (!_loginWindowViewModel.DoctorClient.LoggedIn)
        {
            _loginWindowViewModel.DoctorClient.UserName = _loginWindowViewModel.Username;
            _loginWindowViewModel.DoctorClient.Password = _loginWindowViewModel.SecureStringToString(_loginWindowViewModel.SecurePassword);
            
           
                new Thread(async () =>
                {
                    await _loginWindowViewModel.DoctorClient.AskForLoginAsync();
                }).Start();

            await Task.Delay(1000);
            
            if (_loginWindowViewModel.DoctorClient.LoggedIn)
            {
                await _loginWindowViewModel.DoctorClient.RequestPatientDataAsync();
                await Task.Delay(1000);

                _navigationService.Navigate();
            }
        }
    }
}