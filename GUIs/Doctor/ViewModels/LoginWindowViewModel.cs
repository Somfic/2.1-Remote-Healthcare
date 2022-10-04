using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using RemoteHealthcare.Client;

namespace Doctor.ViewModels;

public class LoginWindowViewModel : ObservableObject
{
    private Client _client;
    public LoginWindowViewModel(Client client)
    {
        _client = client;
        LogIn = new Command(LogInDoctor);
    }

    private string _username;
    private string _password;

    public string Username
    {
        get => _username;
        set => _username = value;
    }

    public string Password
    {
        get => _password;
        set => _password = value;
    }
    public ICommand LogIn { get; }

    void LogInDoctor()
    {
        _client.username = Username;
        _client.password = Password;
        try
        {
            new Thread(async () =>
            {
                await _client.RunAsync();
            }).Start();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}