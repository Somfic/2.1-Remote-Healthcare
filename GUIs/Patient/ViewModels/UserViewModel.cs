using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers.Commands;
using RemoteHealthcare.Client.Client;
using RemoteHealthcare.GUIs.Patient;
using Client = RemoteHealthcare.GUIs.Patient.Client.Client;

namespace Patient.ViewModel
{
    public class UserViewModel : ObservableObject
    {
        
        private string _username;
        private SecureString _password;
        private Client _client;

        public UserViewModel()
        {
            _client = new Client();
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
