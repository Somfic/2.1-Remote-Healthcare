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
using RemoteHealthcare.Common;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient.ViewModels
{
    public class PatientHomepageViewModel : ObservableObject
    {
        private ICommand Send { get; }
        private ICommand ConnectToVr { get; }
        
        private string _message;
        private string _speed= "0km/h";
        private string _distance= "0km";
        private string _time= "0 min";
        private string _heartRate = "0";
        private readonly VrConnection _vr;
        public EngineConnection Engine;
        private string _session;
        
        private ObservableCollection<string> _messages;
        private readonly NavigationStore _navigationStore;
        private readonly Client.Client _client;

        public PatientHomepageViewModel(NavigationStore navigationStore, Client.Client client)
        {
            _client = client;
            _vr = client.VrConnection;
            _messages = new ObservableCollection<string>();
           
            ConnectToVr = new Command(ReconnectToEngine);
            Send = new Command(SendMessage);
            
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }
        
        private void OnCurrentViewModelChanged()
        {
            _client.P = this;
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

        public string HeartRate
        {
            get => _heartRate;

            set
            {
                _heartRate = value + " bpm";
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
                _session = Engine.IsConnected.ToString();
                OnPropertyChanged();
            }
        }

        private void SendMessage()
        {
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.Chat,
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

        private void ReconnectToEngine()
        {
            if (Engine.IsConnected)
            {
                Engine.ConnectAsync();
            }
        }
    }


   
    
    
}
