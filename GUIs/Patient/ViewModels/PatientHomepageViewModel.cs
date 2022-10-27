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
using System.Windows;
using System.Windows.Input;
using NetworkEngine.Socket;
using RemoteHealthcare.Common;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class PatientHomepageViewModel : ObservableObject
    {
        public ObservableCollection<string>_messages;
        
        private string _message;
        private string _messagerecieved;
        private string _speed= "0km/h";
        private string _distance= "0km";
        private string _time= "0 min";
        private string _heartrate = "0";
        private VrConnection _vr;
        public EngineConnection e;
        private string _session;
        
        private readonly NavigationStore _navigationStore;
        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;
        
        private Client.Client _client;

        public PatientHomepageViewModel(NavigationStore navigationStore, Client.Client client)
        {
            
            _client = client;
            _vr = client._vrConnection;
            _messages = new ObservableCollection<string>();
           
            ReconnectVr = new Command(reconnectToEngine);
            Send = new Command(SendMessage);
            
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }
        

        private void OnCurrentViewModelChanged()
        {
            _client.p = this;
            OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
        }
        
        

        public string Speed
        {
            get => _speed;
            set
            {
                _speed = value + "km/h";
                OnPropertyChanged();
            }
        }
        
        public string Distance
        {
            get => _distance;
            set
            {
                _distance = value + "m";
                OnPropertyChanged();
            }
        }

        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        public string Heartrate
        {
            get => _heartrate;

            set
            {
                _heartrate = value + " bpm";
                OnPropertyChanged();
            }
        }


        public ObservableCollection<string> Messages
        {
            get => _messages;

            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }

        
        public string Message
        {
            get => _message;
            set
            {
                _message = value; 
                OnPropertyChanged();
            }
        }

        public string Session
        {
            get => _session;
            set
            {
                _session = value = e._isConnected.ToString();
                OnPropertyChanged();
            }
        }
        

        public ICommand Send { get; }
        public ICommand ReconnectVr { get; }
        void SendMessage()
        {
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _client.UserId,
                    senderName = _client.Username,
                    receiverId = null,
                    message = _message
                }
            };
            _vr.Engine.SendTextToChatPannel($"U: {_message}");
            _client._client.SendAsync(req);
            _messages.Add("You: "+ _message);
            Message = "";
        }
        
        void reconnectToEngine()
        {
            if (e._isConnected)
            {
                e.ConnectAsync();
            }
        }

        public void emergencyStop()
        {
            //message box with emergency stop
            MessageBox.Show("De noodstop is ingedrukt, een assistent zal spoedig mogelijk bij u komen.");
        }
    }


   
    
    
}
