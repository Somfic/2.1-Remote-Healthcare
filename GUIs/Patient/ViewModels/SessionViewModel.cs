<<<<<<< HEAD:GUIs/Patient/ViewModels/SessionViewModel.cs
﻿using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NetworkEngine.Socket;
using RemoteHealthcare.NetworkEngine;
=======
﻿namespace RemoteHealthcare.GUIs.Patient.ViewModels;
>>>>>>> patient_gui:GUIs/Patient/ViewModels/MainViewModel.cs

public class MainViewModel : BaseViewModel
{
<<<<<<< HEAD:GUIs/Patient/ViewModels/SessionViewModel.cs
    public class MainViewModel : ObservableObject, INotifyPropertyChanged
    {
        private ObservableCollection<string>_messages;
        
        private string _message;
        private string _speed= "45km/h";
        private string _distance= "22km";
        private string _time= "33 min";
        private string _heartrate= "126 bpm";
        private VrConnection _vr;
        private EngineConnection e;
        

            private Client.Client _client;

            public MainViewModel(VrConnection vr)
            {
                _vr = vr;
                _client = new Client.Client(null);
                _messages = new ObservableCollection<string>();
                Send = new Command(SendMessage);
                _messages.Add("hello world");
            }

        public MainViewModel(Client.Client client)
        {
            
            _client = client;
            _messages = new ObservableCollection<string>();
            Send = new Command(SendMessage);
            _messages.Add("hello world");

        }

        public MainViewModel()
        {
            
        }
        

        public string Speed
        {
            get
            {
                return _vr.getBikeData().Speed + "mps";
            }
            set
            {
                OnPropertyChanged(_vr.getBikeData().Speed+"mps");
            }
           
        }


        public string Distance
        {
            get => _distance;
            set => _distance = value;
        }
        public string Time
        {
            get => _time;
            set => _time = value;
        }public string Heartrate
        {
            get => _heartrate;
            set => _heartrate = value;
        }

=======
    private readonly NavigationStore _navigationStore;
>>>>>>> patient_gui:GUIs/Patient/ViewModels/MainViewModel.cs

    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;

    public MainViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }
}