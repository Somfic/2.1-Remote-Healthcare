using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Input;
using MvvmHelpers;
using RemoteHealthcare.GUIs.Doctor.Commands;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class LoginWindowViewModel : ObservableObject
{
    public Client _client;
    public ICommand LogIn { get; }

    private string _username;
    private SecureString _password;

    public LoginWindowViewModel(NavigationStore navigationStore)
    {
        _client = new Client();
        LogIn = new LoginCommand(this, 
            new NavigationService<DoctorViewModel>(navigationStore, 
            () => new DoctorViewModel(_client, navigationStore)));
    }

    public string Username
    {
        get => _username;
        set => _username = value;
    }

    public SecureString SecurePassword
    {
        get => _password;
        set => _password = value;
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
        try
        {
            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
            return Marshal.PtrToStringUni(valuePtr);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
        }
    }
}