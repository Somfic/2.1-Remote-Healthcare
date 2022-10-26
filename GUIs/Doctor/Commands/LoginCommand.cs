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
            } catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            await Task.Delay(1000);
            
            if (_loginWindowViewModel._client.loggedIn)
            {
                await _loginWindowViewModel._client.RequestPatientDataAsync();
                await Task.Delay(1000);

                Console.WriteLine("Navigating to DoctorView");
                _navigationService.Navigate();
                Console.WriteLine("Navigated");
            }
        }
    }
}