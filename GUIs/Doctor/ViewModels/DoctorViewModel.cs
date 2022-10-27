using System;
using System.Collections.ObjectModel;
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
    private readonly Client _client;
    private Log _log = new Log(typeof(DoctorViewModel));
    public ICommand EmergencyStop { get; }
    public ICommand SendChatMessage { get; }
    public ICommand StartSessieCommand { get; }
    public ICommand StopSessieCommand { get; }
    public ICommand SetResistanceCommand { get; }
    public ICommand RequestPastSessions { get; }
    
    private Patient _currentUser;
    private string _chatMessage = "";
    private int _resistance = 0;
    
    private ObservableCollection<Patient> _patients;
    public ObservableCollection<string> _chatMessages;
    private SeriesCollection _chartDataSpeed;
    private SeriesCollection _chartDataBPM;

    public DoctorViewModel(Client client, NavigationStore navigationStore)
    {
        _client = client;
        _client.AddDoctorViewmodel(this);
        _patients = new ObservableCollection<Patient>(_client.PatientList);
        _chatMessages = new ObservableCollection<string>();
        EmergencyStop = new EmergencyStopCommand(_client, this);
        SendChatMessage = new SendChatMessageCommand(_client, this);
        StartSessieCommand = new StartSessieCommand(_client, this);
        StopSessieCommand = new StopSessieCommand(_client, this);
        RequestPastSessions = new RequestPastSessions(_client, this);
        SetResistanceCommand = new SetResistanceCommand(_client, this);
    }
    
    public string CurrentUserName
    {
        get => username;
        set
        {
            username = value;
            OnPropertyChanged(nameof(CurrentUserName));
        }
    }

    private string username;
    

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
            
            ChartDataSpeed = new SeriesCollection()
            {
                new LineSeries()
                {
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.DarkSeaGreen,
                    PointGeometrySize = 0,
                    LineSmoothness = 1.00,
                    Values = _currentUser.SpeedData
                }
            };
            
            ChartDataBPM = new SeriesCollection()
            {
                new LineSeries()
                {
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.LightCoral,
                    PointGeometrySize = 0,
                    LineSmoothness = 1.00,
                    Values = _currentUser.BpmData
                }
            };
            OnPropertyChanged();
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

    public int BPM
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
    
    public SeriesCollection ChartDataBPM
    {
        get => _chartDataBPM;
        set
        {
            _chartDataBPM = value;
            OnPropertyChanged();
        }
    }
}