using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MvvmHelpers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.GUIs.Doctor.Commands;
using RemoteHealthcare.Server.Client;
using RemoteHealthcare.Server.Models;
using RemoteHealthcare.Server;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : ObservableObject
{
    private Client _client;
    private Log _log = new Log(typeof(DoctorViewModel));
    public ICommand EmergencyStop { get; }
    public ICommand SendChatMessage { get; }
    public ICommand StartSessieCommand { get; }
    public ICommand StopSessieCommand { get; }
    
    private Patient _currentUser;
    private string _chatMessage;
    private ObservableCollection<Patient> _patients;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;

    public DoctorViewModel(Client client, NavigationStore navigationStore)
    {
        _client = client;
        _patients = new ObservableCollection<Patient>(_client._patientList);
        chatMessages = new ObservableCollection<string>();
        EmergencyStop = new EmergencyStopCommand();
        SendChatMessage = new SendChatMessageCommand(_client, this);
        StartSessieCommand = new StartSessieCommand(_client);
        StopSessieCommand = new StopSessieCommand(_client);
    }

    public Patient CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> ChatMessages
    {
        get => chatMessages;
        set => chatMessages = value;
    }

    public ObservableCollection<Patient> Patients
    {
        get => _patients;
        set => _patients = value;
    }

    public string TextBoxChatMessage
    {
        get => _chatMessage;
        set => _chatMessage = value;
    }
    

    public ChartValues<float> SpeedData { get; set; }

    
}