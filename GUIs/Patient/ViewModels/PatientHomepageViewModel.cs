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
    public class PatientHomepageViewModel : ObservableObject
    {
        public ObservableCollection<string>_messages;
        
        private string _message;
        private string _messagerecieved;
        private string _speed= "45km/h";
        private string _distance= "22km";
        private string _time= "33 min";
        private string _heartrate = "000";
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
           
            test = new Command(testmethode);
            Send = new Command(SendMessage);
            _messages.Add("hello world");
            
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            _client.p = this;
            OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
        }

        public void updateData(string speed)
        {
            Speed = speed;
        }
        
        public string Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged();
            }
        }
        
        public string Distance
        {
            get => _distance;
            set
            {
                _distance = value;
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
                _heartrate = value;
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
                _session = value;
                OnPropertyChanged();
            }
        }
        

        public ICommand Send { get; }
        public ICommand test { get; }
        void SendMessage()
        {
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _client.userId,
                    senderName = _client._username,
                    receiverId = null,
                    message = _message
                }
            };
            _vr.Engine.SendTextToChatPannel($"U: {_message}");
            _client._client.SendAsync(req);
            _messages.Add("You: "+ _message);
            Message = "";
        }
        
        void testmethode()
        {
            if (e._isConnected)
            {
                e.ConnectAsync();
            }
            {
                _vr.Engine.SendTextToChatPannel("test");
            }
            _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
            {
                OpperationCode = OperationCodes.SESSION_START,
            });
        }

        public void emergencyStop()
        {
            throw new NotImplementedException();
        }
    }


   
    
    
}
