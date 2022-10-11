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
        private string _speed;
        private string _distance;
        private string _time;
        private VrConnection vr;
        private EngineConnection e;
        

            private Client.Client _client;
        

        public MainViewModel(Client.Client client)
        {
            
            _client = client;
            _messages = new ObservableCollection<string>();
            Send = new Command(SendMessage);
            _messages.Add("hello world");

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
