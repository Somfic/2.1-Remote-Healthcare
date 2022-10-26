using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers.Commands;
using NetworkEngine.Socket;
using RemoteHealthcare.Common.Data.Providers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.GUIs.Patient.Client;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly NavigationStore _navigationStore;
    private readonly PatientClient _client;

    private VrConnection _vrConnection;

    public LoginViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        LogIn = new Command(LogInPatient);

        _client = new PatientClient(null);
    }

    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;

    public string Username { get; set; }

    public SecureString SecurePassword { get; set; }

    public string BikeId { get; set; }

    public string VrId { get; set; }

    public ICommand LogIn { get; }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
    }

    private async void LogInPatient(object window)
    {
        await _client.Client.ConnectAsync("127.0.0.1", 15243);

        if (!_client.LoggedIn)
        {
            _client.Username = Username;
            _client.Password = SecureStringToString(SecurePassword);

            new Thread(async () => { await _client.PatientLogin(); }).Start();

            await Task.Delay(1000);
            var pvm = new PatientHomepageViewModel(_navigationStore, _client);
            if (_client.LoggedIn)
            {
                _navigationStore.CurrentViewModel = pvm;
                // ((Window) window).Close();
                try
                {
                    var engine = new EngineConnection();
                    await engine.ConnectAsync(VrId);


                    var bike = await DataProvider.GetBike(BikeId);
                    var heart = await DataProvider.GetHeart();
                    _vrConnection = new VrConnection(bike, heart, engine);

                    //The client get the vrConnection 
                    _client.VrConnection = _vrConnection;

                    //Prevends that he GUI patient crash 
                    new Thread(async () => { _vrConnection.Start(pvm); }).Start();

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
        var valuePtr = IntPtr.Zero;
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