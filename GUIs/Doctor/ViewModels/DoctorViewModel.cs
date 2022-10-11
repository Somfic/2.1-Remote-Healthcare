using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MvvmHelpers;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : ObservableObject
{
    private string _doctorName;
    private Patient _currentUser;
    private ObservableCollection<Patient> _users;
    private ObservableCollection<string> chatMessages;
    private ChartValues<float> _speedData;

    public DoctorViewModel()
    {
        _users = new ObservableCollection<Patient>();
        List<Patient> patients = Server.Server._patientData.Patients;
        foreach (ServerClient client in Server.Server._connectedClients)
        {
            foreach (var patient in patients)
            {
                if (client._userId.Equals(patient.UserId))
                {
                    _users.Add(patient);
                }
            }
        }
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

    public ObservableCollection<Patient> Users
    {
        get => _users;
        set => _users = value;
    }

    public ChartValues<float> SpeedData { get; set; }
}