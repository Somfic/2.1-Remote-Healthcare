using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MvvmHelpers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Client;
using RemoteHealthcare.Server.Models;
using RemoteHealthcare.Server;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : ObservableObject
{
    private string _doctorName;
    private Log _log = new Log(typeof(DoctorViewModel));
    private Patient _currentUser;
    private ObservableCollection<Patient> _patients;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;

    public DoctorViewModel()
    {
        
    }

    public string DoctorName
    {
        get => _doctorName;
        set => _doctorName = value;
    }

    public Patient CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
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

    public ChartValues<float> SpeedData { get; set; }
}