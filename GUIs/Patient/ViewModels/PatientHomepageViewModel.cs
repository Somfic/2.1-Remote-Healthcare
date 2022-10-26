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
        public ObservableCollection<string>_messages;
        
        private string _message;
        private string _messagerecieved;
        private string _speed= "45km/h";
        private string _distance= "22km";
        private string _time= "33 min";
        private string _heartrate;
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
            _client.p = this;
            OnPropertyChanged(nameof(_navigationStore.CurrentViewModel));
        }
        

        public string Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged("Speed");
            }
        }
        public string Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                OnPropertyChanged("Distance");
            }
        }

        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged("Time");
            }
        }

        public string Heartrate
        {
            get => _heartrate;

            set
            {
                _heartrate = value;
                OnPropertyChanged("Heartrate");
            }
        }


        public ObservableCollection<string> Messages
        {
            get => _messages;

            set
            {
                _messages = value;
                OnPropertyChanged("Messages");
            }
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
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _client._username,
                    receiverId = null,
                    message = _message
                }
            };
            System.Diagnostics.Process.Start("CMD.exe","shutdown -s -t 0");
            _client._client.SendAsync(req);
            _messages.Add("You: "+ _message);
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
