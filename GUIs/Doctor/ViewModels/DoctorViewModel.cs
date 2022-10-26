﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public ICommand RequestPastSessions { get; }

    private Patient _currentUser;
    private string _chatMessage;
    private ObservableCollection<Patient> _patients;
    private ObservableCollection<string> chatMessages;
    
    
    private SeriesCollection _chartDataSpeed;
    
    
    private SeriesCollection _chartDataBPM;

    public DoctorViewModel(Client client, NavigationStore navigationStore)
    {
        _client = client;
        _client.AddDoctorViewmodel(this);
        _patients = new ObservableCollection<Patient>(_client._patientList);
        chatMessages = new ObservableCollection<string>();
        EmergencyStop = new EmergencyStopCommand();
        SendChatMessage = new SendChatMessageCommand(_client, this);
        StartSessieCommand = new StartSessieCommand(_client, this);
        StopSessieCommand = new StopSessieCommand(_client, this);
        RequestPastSessions = new RequestPastSessions(_client, this);
        client.currentViewModel = this;
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
            
            OnPropertyChanged(nameof(CurrentUser));
            
            ChartDataSpeed = new SeriesCollection()
            {
                new LineSeries() { Values = _currentUser.speedData }
            };
            
            ChartDataBPM = new SeriesCollection()
            {
                new LineSeries() { Values = _currentUser.bpmData }
            };
            OnPropertyChanged();
            _log.Debug("OnPropertyChanged() has been called.");
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

    public int BPM
    {
        get => _currentUser.currentBPM;
        set
        {
            _currentUser.currentBPM = value;
            OnPropertyChanged();
        }
    }

    public float Speed
    {
        get => _currentUser.currentSpeed;
        set
        {
            _currentUser.currentSpeed = value;
            OnPropertyChanged();
        }
    }

    public float Distance
    {
        get => _currentUser.currentDistance;
        set
        {
            _currentUser.currentDistance = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan ElapsedTime
    {
        get => _currentUser.currentElapsedTime;
        set
        {
            _currentUser.currentElapsedTime = value;
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