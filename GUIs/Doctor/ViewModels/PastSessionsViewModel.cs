using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using MvvmHelpers;
using LiveCharts;
using LiveCharts.Wpf;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class PastSessionsViewModel : ObservableObject
{
    private Client _client;

    private string _distance;
    private TimeSpan _totalTime;

    private SessionData _currentSession;
    private ObservableCollection<SessionData> _sessions;
    private ChartValues<float> _bpm;
    private ChartValues<float> _speed;
    private SeriesCollection _speedData;
    private SeriesCollection _bpmData;
    private string _userName;
    private string _sessionName;

    public PastSessionsViewModel(Client client, string userName)
    {
        _client = client;
        _client.AddPastSessionsViewmodel(this);
        _sessions = new ObservableCollection<SessionData>(_client.Sessions);
        _distance = "0";
        _totalTime = new(0);
        _bpm = new();
        _speed = new();
        _userName = userName;
    }

    public SessionData CurrentSession
    {
        get => _currentSession;
        set
        {
            _currentSession = value;
            SessionName = value.ToString();
            SpeedCollection = FillValues("speed");
            BpmCollection = FillValues("bpm");
            TotalTime = TimeSpan.FromSeconds(_currentSession.MiniDatas.Count);
            TotalDistance = CalculateTotalDistance();

            SpeedData = new SeriesCollection()
            {
                new LineSeries()
                {
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.DarkSeaGreen,
                    PointGeometrySize = 0,
                    LineSmoothness = 1.00,
                    Values = _speed,
                }
            };
            BpmData = new SeriesCollection()
            {
                new LineSeries()
                {
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.LightCoral,
                    PointGeometrySize = 0,
                    LineSmoothness = 1.00, 
                    Values = _bpm
                }
            };

            OnPropertyChanged();
        }
    }

    public ObservableCollection<SessionData> Sessions
    {
        get => _sessions;
        set
        {
            _sessions = value;
            OnPropertyChanged();
        }
    }

    public string UserName
    {
        get => _userName;
        set
        {
            _userName = value;
            OnPropertyChanged();
        }
    }

    public string SessionName
    {
        get => _sessionName;
        set
        {
            _sessionName = value;
            OnPropertyChanged();
        }
    }

    public ChartValues<float> BpmCollection
    {
        get => _bpm;
        set
        {
            _bpm = value;
            OnPropertyChanged();
        }
    }

    public ChartValues<float> SpeedCollection
    {
        get => _speed;
        set
        {
            _speed = value;
            OnPropertyChanged();
        }
    }

    public SeriesCollection SpeedData
    {
        get => _speedData;
        set
        {
            _speedData = value;
            OnPropertyChanged();
        }
    }

    public SeriesCollection BpmData
    {
        get => _bpmData;
        set
        {
            _bpmData = value;
            OnPropertyChanged();
        }
    }

    public string TotalDistance
    {
        get => $"{_distance} m";

        set
        {
            _distance = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan TotalTime
    {
        get => _totalTime;
        set
        {
            _totalTime = value;
            OnPropertyChanged();
        }
    }

    private ChartValues<float> FillValues(string returnType)
    {
        ChartValues<float> values = new();
        foreach (var data in _currentSession.MiniDatas)
        {
            if (returnType.ToLower().Equals("speed"))
                values.Add(data.Speed);
            else if (returnType.ToLower().Equals("bpm"))
                values.Add(data.Heartrate);
        }

        return values;
    }

    private string CalculateTotalDistance()
    {
        return _currentSession.MiniDatas.Last().Distance.ToString();
        /*int dist = 0;
        int? prefValue = null;

        foreach (var data in _currentSession.MiniDatas)
        {
            int currentValue = data.Distance;
            
            if (prefValue == null)
            {
                prefValue = 0;
                dist -= data.Distance;
                continue;
            }
            else if (prefValue >= 200 && currentValue <= 100)
                dist += (255 - prefValue.Value) + currentValue;
            else if (currentValue <= 255)
                dist += currentValue - prefValue.Value;

            prefValue = currentValue;
        }
        return $"{dist}";*/
    }
}