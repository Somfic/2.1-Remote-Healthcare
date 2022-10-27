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
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Data.Providers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        
        private string _username;
        private SecureString _securePassword;
        private string _bikeId;
        private string _vrId;
        private readonly Client.Client _client;
        private VrConnection _vrConnection;

        public ICommand LogIn { get; }

        private readonly NavigationStore _navigationStore;
        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;
        
        public LoginViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            
            LogIn = new Command(LogInPatient);

            _client = new Client.Client();
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
            get => _bikeId;
            set => _bikeId = value;
        }

        public string VrId
        {
            get => _vrId;
            set => _vrId = value;
        }

        /// <summary>
        /// It connects to the server, logs in the patient, connects to the engine, and starts the VR connection
        /// </summary>
        /// <param name="window">The window that is currently open.</param>
        async void LogInPatient(object window)
        {
            await _client._client.ConnectAsync("127.0.0.1", 15243);
            Console.WriteLine("Got window, logging in patient");
            if (!_client.LoggedIn)
            {
                _client.Username = Username;
                _client.Password = SecureStringToString(SecurePassword);
                
                try {
                    new Thread(async () => { await _client.PatientLogin(); }).Start();
                } catch (Exception exception) {
                    Console.WriteLine(exception);
                    throw;
                }

                await Task.Delay(1000);
               
                if (_client.LoggedIn)
                {
                    var engine = new EngineConnection();
                    var bike = await DataProvider.GetBike(_bikeId);
                    var heart = await DataProvider.GetHeart();
                    _vrConnection = new VrConnection(bike, heart, engine);
                    _client.VrConnection = _vrConnection;

                    PatientHomepageViewModel pvm = new PatientHomepageViewModel(_navigationStore, _client);
                    
                    _navigationStore.CurrentViewModel = pvm;
                    pvm.Engine = engine;
                    
                    try
                    {
                        await engine.ConnectAsync(_vrId);
                        new Thread(async () => { _vrConnection.Start(pvm); }).Start();
                        await Task.Delay(-1);
                    }
                    catch (Exception ex)
                    {
                        var log = new Log(typeof(LoginViewModel));
                        log.Critical(ex, "Program stopped because of exception");
                    }
                }
                else
                {
                    MessageBox.Show("wrong login");
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
