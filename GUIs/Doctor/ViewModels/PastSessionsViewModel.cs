using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MvvmHelpers;
using LiveCharts;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class PastSessionsViewModel : ObservableObject, INotifyPropertyChanged
{
    private Client _client;
    private Log _log = new Log(typeof(PastSessionsWindow));

    private string _distance;
    private TimeSpan _totalTime;

    private SessionData _currentSession;
    private string _chatMessage;
    private ObservableCollection<SessionData> _sessions;
    private ObservableCollection<float> _bpm;
    private ObservableCollection<float> _speed;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;
    private string _userName;
    private string _sessionName;

    public PastSessionsViewModel(Client client, string userName)
    {
        _client = client;
        _client.AddPastSessionsViewmodel(this);
        _sessions = new ObservableCollection<SessionData>(_client.Sessions);
        _distance = "0";
        _totalTime = new(0);
        _userName = userName;
    }

    public SessionData CurrentSession
    {
        get
        {
            _log.Information($"CurrentSession:get {_currentSession}");
            return _currentSession;
        }
        set
        {
            _log.Information($"CurrentSession:set {value}");
            _currentSession = value;
            SessionName = value.ToString();
            SpeedCollection = fillCollection("speed");
            BpmCollection = fillCollection("bpm");
            TotalTime = TimeSpan.FromSeconds(_currentSession.MiniDatas.Count);
            _log.Debug(
                $"Current session is now {value}; SessionName = {_sessionName}; Speed has a count of {_speed.Count}; " +
                $"Bpm has a count of {_bpm.Count}; TotalTime = {_totalTime}");

            OnPropertyChanged();
            _log.Debug("OnPropertyChanged() has been called.");
        }
    }

    public ObservableCollection<SessionData> Sessions
    {
        get
        {
            _log.Information($"Session:get {_sessions}");
            return _sessions;
        }
        set
        {
            _log.Information($"Session:set {value}");
            _sessions = value;
            OnPropertyChanged();
        }
    }

    public string UserName
    {
        get
        {
            _log.Information($"UserName:get {_userName}");
            return _userName;
        }
        set
        {
            _log.Information($"UserName:set {value}");
            _userName = value;
            OnPropertyChanged();
        }
    }

    public string SessionName
    {
        get
        {
            _log.Information($"SessionName:get {_sessionName}");
            return _sessionName;
        }
        set
        {
            _log.Information($"SessionName:set {value}");
            _sessionName = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<float> BpmCollection
    {
        get
        {
            _log.Information($"BpmCollection:get {_bpm.Count}");
            return _bpm;
        }
        set
        {
            _log.Information($"BpmCollection:set {value.Count}");
            _bpm = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<float> SpeedCollection
    {
        get
        {
            _log.Information($"SpeedCollection:get {_speed.Count}");
            return _speed;
        }
        set
        {
            _speed = value;
            OnPropertyChanged();
        } 
    }

    public string TotalDistance
    {
        get
        {
            _log.Information($"TotalDistance:get {_distance}");
            return $"{_distance} m";
        }
        set
        {
            _distance = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan TotalTime
    {
        get
        {
            _log.Information($"TotalTime:get {_totalTime}");
            return _totalTime;
        }
        set
        {
            _totalTime = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<float> fillCollection(string returnType)
    {
        ObservableCollection<float> collection = new();
        foreach (var session in _currentSession.MiniDatas)
        {
            if (returnType.ToLower().Equals("speed"))
                collection.Add(session.Speed);
            else if (returnType.ToLower().Equals("bpm"))
                collection.Add(session.Heartrate);
        }

        return collection;
    }
}