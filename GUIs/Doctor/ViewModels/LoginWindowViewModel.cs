using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class LoginWindowViewModel : ObservableObject
{
    private Client.Client _client;
    public LoginWindowViewModel()
    {
        _client = new Client.Client();
        LogIn = new Command(LogInDoctor);
    }

    private string _username;
    private SecureString _password;

    public string Username
    {
        get => _username;
        set => _username = value;
    }

    public SecureString SecurePassword
    {
        private get => _password;
        set => _password = value;
    }
    public ICommand LogIn { get; }

    void LogInDoctor(object window)
    {
        Window windowToClose = window as Window;
        _client.username = Username;
        _client.password = SecureStringToString(SecurePassword);
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

        DoctorView doctorView = new DoctorView();
        windowToClose.Close();
        doctorView.Show();
    }

    public string SecureStringToString(SecureString value)
    {
        IntPtr valuePtr = IntPtr.Zero;
        try {
            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
            return Marshal.PtrToStringUni(valuePtr);
        } finally {
            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
        }
    }
}