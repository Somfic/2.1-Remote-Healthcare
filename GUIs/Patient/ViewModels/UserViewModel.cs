using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class UserViewModel : ObservableObject
    {
        
        private string _username;
        private SecureString _securePassword;
        private Client.Client _client;

        public UserViewModel()
        {
            LogIn = new Command(LogInPatient);
            _client = new Client.Client();
            
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public SecureString SecurePassword
        {
            get => _securePassword;
            set => _securePassword = value;
        }
        
        public ICommand LogIn { get; }
        
      async void LogInPatient(object window)
        { 
            Window windowToClose = window as Window;
            await _client.client.ConnectAsync("127.0.0.1", 15243);
            Console.WriteLine("Got window, logging in patient");
            _client.username = Username;
            
            _client.password = SecureStringToString(SecurePassword);
       
            Console.WriteLine(_client.password);
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

            MainViewModel m = new MainViewModel(_client);
            PatientView patientView= new PatientView();
            patientView.DataContext = m;
            // windowToClose.Close();
            patientView.Show();
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
    
}
