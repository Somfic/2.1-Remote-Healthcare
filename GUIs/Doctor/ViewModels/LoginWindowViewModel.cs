using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers.Commands;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class LoginWindowViewModel : BaseViewModel
{
    private Client.Client _client;

    private readonly NavigationStore _navigationStore;

    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;
    public ICommand LogIn { get; }

    public LoginWindowViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

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


 
    async void LogInDoctor(object window)
    {
        Console.WriteLine("LogInDocotr");
        //Window windowToClose = window as Window;
        await _client._client.ConnectAsync("127.0.0.1", 15243);

        if (!_client.loggedIn)
        {
            _client.username = Username;
            _client.password = SecureStringToString(SecurePassword);
            try
            {
                new Thread(async () => { await _client.AskForLoginAsync(); }).Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            await Task.Delay(1000);

            if (_client.loggedIn)
            {
                _navigationStore.CurrentViewModel = new DoctorViewModel(_navigationStore);
                

                // _client.RequestClients();
                //DoctorViewModel doctorViewModel = new DoctorViewModel();
                //DoctorViewModel doctorViewModel = new DoctorViewModel();
                //DoctorView doctorView = new DoctorView();
                //doctorView.DataContext = doctorViewModel;
                //windowToClose.Close();
                //doctorView.Show();
            }
        }
    }
    
    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
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