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

public class DoctorViewModel : ObservableObject
{
    private string _doctorName;
    private UserModel _currentUser;
    private ObservableCollection<Patient> _users;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;
    private Client.Client _doctorClient; 
    public ICommand SessionStartCommand { get; }


    public DoctorViewModel()
    {
        this._currentUser = new UserModel();
        //SessionStartCommand = new Command(SessieStart);
    }

    async void SessieStart()
    {
        var req = new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_START,
        };

        await _doctorClient._client.SendAsync(req);
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