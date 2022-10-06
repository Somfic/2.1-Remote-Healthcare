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
        private SecureString _password;
        private Client.Client _client;

        public UserViewModel()
        {
            _client = new Client.Client();
            LogIn = new Command(LogInPatient);
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public SecureString Password
        {
            get => _password;
            set => _password = value;
        }
        
        public ICommand LogIn { get; }
        
        void LogInPatient(object window)
        { 
            Window windowToClose = window as Window;
            Console.WriteLine("Got window, logging in patient");
            _client.username = Username;
            _client.password = SecureStringToString(Password);
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

            PatientView patientView= new PatientView();
            windowToClose.Close();
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
