using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NetworkEngine.Socket;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ObservableCollection<string>_messages;
        
        private string _message;
        private string _speed= "45km/h";
        private string _distance= "22km";
        private string _time= "33 min";
        private string _heartrate= "126 bpm";
        private VrConnection vr;
        private EngineConnection e;
        

            private Client.Client _client;

            public MainViewModel()
            {
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
            set => _heartrate = value;
        }


        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => _messages = value;
        }

        
        public string Message
        {
            get => _message;
            set => _message = value;
        }

        public ICommand Send { get; }
        void SendMessage()
        {
            _messages.Add("You: "+_message);
        }

    }


   
    
    
}
