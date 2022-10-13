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
        private string _bikeID;
        private string _Vrid;
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

        public string BikeId
        {
            get => _bikeID;
            set => _bikeID = value;
        }

        public string VrId
        {
            get => _Vrid;
            set => _Vrid = value;
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
                    new Thread(async () => { await _client.PatientLogin(); }).Start();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            
            

               
                try
                {
                    // var engine = new EngineConnection();
                    // await engine.ConnectAsync();

                    Console.WriteLine("Enter Bike ID:");
                    var bike = await DataProvider.GetBike(_bikeID);
                    var heart = await DataProvider.GetHeart();
                    vrConnection = new VrConnection(bike, heart);
                    // vrConnection.Start();
                    MainViewModel m = new MainViewModel(vrConnection);
                    PatientView patientView = new PatientView();
                    patientView.DataContext = m;
                    // windowToClose.Close();
                    patientView.Show();

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
