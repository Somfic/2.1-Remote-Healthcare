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
        await _loginWindowViewModel.Client._client.ConnectAsync("127.0.0.1", 15243);

        if (!_loginWindowViewModel.Client.LoggedIn)
        {
            _loginWindowViewModel.Client.Username = _loginWindowViewModel.Username;
            _loginWindowViewModel.Client.Password = _loginWindowViewModel.SecureStringToString(_loginWindowViewModel.SecurePassword);
            
            try
            {
                new Thread(async () =>
                {
                    await _loginWindowViewModel.Client.AskForLoginAsync();
                }).Start();
            } catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            await Task.Delay(1000);
            
            if (_loginWindowViewModel.Client.LoggedIn)
            {
                await _loginWindowViewModel.Client.RequestPatientDataAsync();
                await Task.Delay(1000);

                _navigationService.Navigate();
            }
        }
    }
}