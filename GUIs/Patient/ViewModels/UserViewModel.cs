using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using NetworkEngine.Socket;
using RemoteHealthcare.Client.Data.Providers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class UserViewModel : ObservableObject
    {
        
        private string _username;
        private SecureString _securePassword;
        private Client.Client _client;
        private VrConnection vrConnection;

        public UserViewModel()
        {
            LogIn = new Command(LogInPatient);

            _client = new Client.Client(null);

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
            
            await _client._client.ConnectAsync("127.0.0.1", 15243);
            Console.WriteLine("Got window, logging in patient");
            _client._username = Username;
            
            _client._password = SecureStringToString(SecurePassword);
       
            Console.WriteLine(_client._password);
            try
            {
                new Thread(async () =>
                {
                    await _client.PatientLogin();
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
            try
            {
                var engine = new EngineConnection();
                await engine.ConnectAsync();

                Console.WriteLine("Enter Bike ID:");
                var bike = await DataProvider.GetBike(Console.ReadLine());
                var heart = await DataProvider.GetHeart();
                vrConnection = new VrConnection(bike, heart, engine);
                vrConnection.Start();


                // _client = new Client.Client(vrConnection);
                
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                var log = new Log(typeof(UserViewModel));
                log.Critical(ex, "Program stopped because of exception");
            }
           
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
