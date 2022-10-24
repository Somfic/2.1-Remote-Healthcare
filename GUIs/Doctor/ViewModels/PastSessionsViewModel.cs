using System;
using System.Collections.ObjectModel;
using MvvmHelpers;
using LiveCharts;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class PastSessionsViewModel : ObservableObject
{
    private Client _client;
    private Log _log = new Log(typeof(PastSessionsWindow));
    
    private float _distance = 0;
    private TimeSpan _elapsed = new TimeSpan(0);
    
    private SessionData _currentSession;
    private string _chatMessage;
    private ObservableCollection<SessionData> _sessions;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;
    private string _userName;

    public PastSessionsViewModel(Client client, string userName)
    {
        _log.Critical("constructor");
        _client = client;
        _client.AddPastSessionsViewmodel(this);
        _sessions = new ObservableCollection<SessionData>(_client.Sessions);
        _userName = userName;
    }

    public SessionData CurrentSession
    {
        get => _currentSession;
        set
        {
            _currentSession = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SessionData> Sessions
    {
        get => _sessions;
        set => _sessions = value;
    }

    public float TotalDistance
    {
        get => _distance;
        set => _distance = value;
    }

    public TimeSpan totalTime
    {
        get => _elapsed;
        set => _elapsed = value;
    }
}