﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using MvvmHelpers.Commands;
using NetworkEngine.Socket;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Patient.Client;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels;

public class PatientHomepageViewModel : BaseViewModel
{
    private readonly NavigationStore _navigationStore;

    private readonly PatientClient _client;
    private string _distance = "22km";
    private EngineConnection _e;
    private string _heartrate;

    private string _message;
    private string _messagerecieved;
    private ObservableCollection<string> _messages;
    private string _session;
    private string _speed = "45km/h";
    private string _time = "33 min";
    private VrConnection _vr;

    public PatientHomepageViewModel(NavigationStore navigationStore, PatientClient client)
    {
        _client = client;
        _vr = client.VrConnection;
        _messages = new ObservableCollection<string>();

        Test = new Command(Testmethode);
        Send = new Command(SendMessage);
        _messages.Add("hello world");

        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }

    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;


    public string Speed
    {
        get => _speed;
        set
        {
            _speed = value + "km/h";
            OnPropertyChanged("Speed");
        }
    }

    public string Distance
    {
        get => _distance;
        set
        {
            _distance = value + "m";
            OnPropertyChanged("Distance");
        }
    }

    public string Time
    {
        get => _time;
        set
        {
            _time = value;
            OnPropertyChanged("Time");
        }
    }

    public string Heartrate
    {
        get => _heartrate;

        set
        {
            _heartrate = value + " bpm";
            OnPropertyChanged("Heartrate");
        }
    }


    public ObservableCollection<string> Messages
    {
        get => _messages;

        set
        {
            _messages = value;
            OnPropertyChanged("Messages");
        }
    }


    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged("Message");
        }
    }

    public string Session
    {
        get => _session;
        set
        {
            _session = value;
            OnPropertyChanged("Session");
        }
    }


    public ICommand Send { get; }
    public ICommand Test { get; }

    private void OnCurrentViewModelChanged()
    {
        _client.P = this;
        OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
    }

    private void SendMessage()
    {
        var req = new DataPacket<ChatPacketRequest>
        {
            OpperationCode = OperationCodes.Chat,
            Data = new ChatPacketRequest
            {
                SenderId = _client.Username,
                ReceiverId = null,
                Message = _message
            }
        };
        _client.Client.SendAsync(req);
        _messages.Add("You: " + _message);
        //clear textbox
        Message = "";
    }

    private void Testmethode()
    {
        _client.Client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SessionStart
        });
    }
}