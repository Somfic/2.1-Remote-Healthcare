﻿using RemoteHealthcare.GUIs.Doctor.Models;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : ObservableObject
{
    private string _doctorName;
    private UserModel _currentUser;
    private ObservableCollection<Patient> _users;
    private ObservableCollection<string> _chatMessages;
    private ChartValues<float> _speedData;

    public DoctorViewModel()
    {
        this._currentUser = new UserModel();
    }
    
    public string DoctorName
    {
        get => _doctorName;
        set => _doctorName = value;
    }

    public UserModel CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
    }

    public ObservableCollection<string> ChatMessages
    {
        get => _chatMessages;
        set => _chatMessages = value;
    }

    public ObservableCollection<Patient> Users
    {
        get => _users;
        set => _users = value;
    }

    public ChartValues<float> SpeedData { get; set; }
}