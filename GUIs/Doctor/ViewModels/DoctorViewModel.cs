using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MvvmHelpers;
using MvvmHelpers.Commands;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.GUIs.Doctor.Models;
using RemoteHealthcare.Server.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : BaseViewModel
{
    private string _doctorName;
    private UserModel _currentUser;
    private ObservableCollection<Patient> _users;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;
    private Client.Client _doctorClient; 
    public ICommand SessionStartCommand { get; }

    private readonly NavigationStore _navigationStore;

    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;
    
    public DoctorViewModel(NavigationStore navigationStore, Client.Client client)
    {
        this._currentUser = new UserModel();
        SessionStartCommand = new Command(SessieStart);

        _doctorClient = client;
        
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }
    
    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
    }

    async void SessieStart()
    {
        await _doctorClient.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_START,
        },sessie_callback );
    }


    private void sessie_callback(DataPacket obj)
    {
        Console.WriteLine("hoii sanhdklsadgkfjmdsakjfghk");
        Console.WriteLine(obj.GetData<SessionStartPacketResponse>().message);
        Console.WriteLine(obj.GetData<SessionStartPacketResponse>().statusCode);
    }

    public string DoctorName
    {
        get => _doctorName;
        set => _doctorName = value;
    }
    
    
    public Client.Client DoctorClient
    {
        get => _doctorClient;
        set => _doctorClient = value;
    }

    public UserModel CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
    }

    public ObservableCollection<string> ChatMessages
    {
        get => chatMessages;
        set => chatMessages = value;
    }

    public ObservableCollection<Patient> Users
    {
        get => _users;
        set => _users = value;
    }

    public ChartValues<float> SpeedData { get; set; }
}