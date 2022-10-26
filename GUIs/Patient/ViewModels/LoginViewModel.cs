using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System.Windows.Input;
using NetworkEngine.Socket;
using RemoteHealthcare.Client.Data.Providers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        
        private string _username;
        private SecureString _securePassword;
        private string _bikeID;
        private string _Vrid;
        private Client.Client _client;
        private VrConnection vrConnection;

        private readonly NavigationStore _navigationStore;
        public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;
        
        public LoginViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            
            LogIn = new Command(LogInPatient);

            _client = new Client.Client(null);

        }
        
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
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
            if (!_client._loggedIn)
            {
                _client._username = Username;
                _client._password = SecureStringToString(SecurePassword);
                
                try {
                    new Thread(async () => { await _client.PatientLogin(); }).Start();
                } catch (Exception exception) {
                    Console.WriteLine(exception);
                    throw;
                }

                await Task.Delay(1000);
                PatientHomepageViewModel pvm = new PatientHomepageViewModel(_navigationStore, _client);
                if (_client._loggedIn)
                {
                    _navigationStore.CurrentViewModel = pvm;
                    // ((Window) window).Close();
                    try
                    {
                        var engine = new EngineConnection(); 
                        await engine.ConnectAsync();
                        // // Console.WriteLine("Enter Bike ID:");
                        var bike = await DataProvider.GetBike(_bikeID);
                        var heart = await DataProvider.GetHeart();
                        vrConnection = new VrConnection(bike, heart, engine);
                        
                        _client._vrConnection = vrConnection;
                        new Thread(async => {vrConnection.Start(pvm); }).Start();
                      
                      

                        await Task.Delay(-1);
                    }
                    catch (Exception ex)
                    {
                        var log = new Log(typeof(LoginViewModel));
                        log.Critical(ex, "Program stopped because of exception");
                    }
                }
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
