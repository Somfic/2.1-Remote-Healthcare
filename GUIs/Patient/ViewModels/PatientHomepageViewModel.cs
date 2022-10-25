using MvvmHelpers;
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
using RemoteHealthcare.Common;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class PatientHomepageViewModel : BaseViewModel
    {
        private ObservableCollection<string>_messages;
        
        private string _message;
        private string _speed= "45km/h";
        private string _distance= "22km";
        private string _time= "33 min";
        private string _heartrate= "126 bpm";
        private VrConnection _vr;
        private EngineConnection e;
        
        private readonly NavigationStore _navigationStore;
        public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;
        
        private Client.Client _client;

        public PatientHomepageViewModel(NavigationStore navigationStore, Client.Client client)
        {
            
            _client = client;
            _vr = client._vrConnection;
            _messages = new ObservableCollection<string>();
            test = new Command(testmethode);
            Send = new Command(SendMessage);
            _messages.Add("hello world");
            
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
        }
        

        public string Speed
        {
            get => _speed;
            set => _speed = value;
        }
        public string Distance
        {
            get => _distance;
            set => _distance = value;
        }public string Time
        {
            get => _time;
            set => _time = value;
        }public string Heartrate
        {
            get => _heartrate;
            set
            {
                _heartrate = value = _vr.getHearthData().HeartRate.ToString();
                OnPropertyChanged("Heartrate");
            } 
        }


        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => _messages = value;
        }

        
        public string Message
        {
            get => _message;
            set
            {
                _message = value; 
                OnPropertyChanged("Message");
            }
            


        }

        public ICommand Send { get; }
        public ICommand test { get; }
        void SendMessage()
        {
            _messages.Add("You: "+_message);
            //clear textbox
            Message = "";
            
        }
        
        void testmethode()
        {
            _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
            {
                OpperationCode = OperationCodes.SESSION_START,
            });
        }

    }


   
    
    
}
