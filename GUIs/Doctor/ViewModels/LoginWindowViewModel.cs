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

    /// <summary>
    /// It takes a window object, closes it, and opens a new window
    /// </summary>
    /// <param name="window">The window that is currently open.</param>
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

    /// <summary>
    /// "Convert a SecureString to a string by copying the SecureString to unmanaged memory, then copying the unmanaged
    /// memory to a managed string, then zeroing out the unmanaged memory."
    /// 
    /// The first thing to notice is that the function returns a string.  This is the string that you want to use in your
    /// code.  The SecureString is only used to get the string.  The SecureString is not used in the code that uses the
    /// string
    /// </summary>
    /// <param name="SecureString">The SecureString object that you want to convert to a string.</param>
    /// <returns>
    /// A string
    /// </returns>
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