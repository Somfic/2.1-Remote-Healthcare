using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using MvvmHelpers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.GUIs.Doctor.Commands;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : ObservableObject
{
    private DoctorClient _doctorClient;
    private Log _log = new Log(typeof(DoctorViewModel));
    public ICommand EmergencyStop { get; }
    public ICommand SendChatMessage { get; }
    public ICommand StartSessieCommand { get; }
    public ICommand StopSessieCommand { get; }
    public ICommand SetResistanceCommand { get; }
    public ICommand RequestPastSessions { get; }
    
    private Patient _currentUser;
    private string _chatMessage = "";
    private int _resistance;
    
    private ObservableCollection<Patient> _patients;
    private ObservableCollection<string> _chatMessages;
    private SeriesCollection _chartDataSpeed;
    private SeriesCollection _chartDataBpm;

    public DoctorViewModel(DoctorClient doctorClient, NavigationStore navigationStore)
    {
        _doctorClient = doctorClient;
        _doctorClient.AddDoctorViewmodel(this);
        _patients = new ObservableCollection<Patient>(_doctorClient.PatientList);
        _chatMessages = new ObservableCollection<string>();
        EmergencyStop = new EmergencyStopCommand(_doctorClient, this);
        SendChatMessage = new SendChatMessageCommand(_doctorClient, this);
        StartSessieCommand = new StartSessieCommand(_doctorClient, this);
        StopSessieCommand = new StopSessieCommand(_doctorClient, this);
        RequestPastSessions = new RequestPastSessions(_doctorClient, this);
        SetResistanceCommand = new SetResistanceCommand(_doctorClient, this);
    }
    
    public string CurrentUserName
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    private string _username;
    

    public Patient CurrentUser
    {
        get => _currentUser;
        set
        {
            
            _currentUser = value;
            if (CurrentUser != null)
            {
                CurrentUserName = CurrentUser.Username;
            }
            
            //OnPropertyChanged(nameof(CurrentUser));
            
            ChartDataSpeed = new SeriesCollection
            {
                new LineSeries
                {
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.DarkSeaGreen,
                    PointGeometrySize = 0,
                    LineSmoothness = 1.00,
                    Values = _currentUser.SpeedData
                }
            };
            
            ChartDataBpm = new SeriesCollection
            {
                new LineSeries
                {
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.LightCoral,
                    PointGeometrySize = 0,
                    LineSmoothness = 1.00,
                    Values = _currentUser.BpmData
                }
            };
            OnPropertyChanged();
            _log.Debug("OnPropertyChanged() has been called.");
        }
    }

    public ObservableCollection<string> ChatMessages
    {
        get => _chatMessages;
        set
        {
            _chatMessages = value;
            OnPropertyChanged();
        } 
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

    public int Resistance
    {
        get => _resistance;
        set => _resistance = value;
           
    }

    public int Bpm
    {
        get => _currentUser.CurrentBpm;
        set
        {
            _currentUser.CurrentBpm = value;
            OnPropertyChanged();
        }
    }

    public float Speed
    {
        get => _currentUser.CurrentSpeed;
        set
        {
            _currentUser.CurrentSpeed = value;
            OnPropertyChanged();
        }
    }

    public float Distance
    {
        get => _currentUser.CurrentDistance;
        set
        {
            _currentUser.CurrentDistance = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan ElapsedTime
    {
        get => _currentUser.CurrentElapsedTime;
        set
        {
            _currentUser.CurrentElapsedTime = value;
            OnPropertyChanged();
        }
    }

    public SeriesCollection ChartDataSpeed
    {
        get => _chartDataSpeed;
        set
        {
            _chartDataSpeed = value;
            OnPropertyChanged();
        }
    }
    
    public SeriesCollection ChartDataBpm
    {
        get => _chartDataBpm;
        set
        {
            _chartDataBpm = value;
            OnPropertyChanged();
        }
    }

    public void AddMessage(string message)
    {
        _log.Information($"addmessage, {ChatMessages.Count}; {message}");
        BindingOperations.EnableCollectionSynchronization(ChatMessages, message);
        _log.Information($"added message, {ChatMessages.Count}");
        // chatMessages.Add(message);
    }
}