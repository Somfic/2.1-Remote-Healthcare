using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MvvmHelpers;
using RemoteHealthcare.GUIs.Doctor.Models;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel : ObservableObject
{
    private string _doctorName;
    private UserModel _currentUser;
    private ObservableCollection<string> _users;
    private ChartValues<float> _speedData;

    public DoctorViewModel()
    {
    }
    
    public string DoctorName
    {
        get => _doctorName;
        set => _doctorName = value;
    }

    public ObservableCollection<string> Users
    {
        get => _users;
        set => _users = value;
    }

    public ChartValues<float> SpeedData { get; set; }
}