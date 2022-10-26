using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using NetworkEngine.Socket;
using RemoteHealthcare.GUIs.Patient.ViewModels;
using System.Collections;
using RemoteHealthcare.Common.Data;

namespace RemoteHealthcare.GUIs.Patient.Client
{
   public class Client
    {
        public SocketClient _client = new(true);
        private Log _log = new(typeof(Client));

        public bool _loggedIn;
        public string _password;
        public string _username;
        private string userId;
        private string doctorId;
        private string _sessionId;
        public PatientHomepageViewModel p;
        private Boolean _sessienRunning = false;

        private Dictionary<string, Action<DataPacket>> _callbacks;
        public VrConnection _vrConnection;
        public  Client(VrConnection v)
        {
            
            _loggedIn = false;
            _callbacks = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _callbacks.Add("login", LoginFeature);
            _callbacks.Add("chat", ChatHandlerAsync);
            _callbacks.Add("session start", SessionStartHandler);
            _callbacks.Add("session stop", SessionStopHandler);
            _callbacks.Add("disconnect", DisconnectHandler);
            _callbacks.Add("set resitance", SetResistanceHandeler);
            _callbacks.Add("emergency stop", EmergencyStopHandler);

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };
            
            _sessionId = DateTime.Now.ToString();

        }

        public async Task PatientLogin()
        {
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                Data = new LoginPacketRequest()
                {
                    UserName = _username,
                    Password = _password,
                    IsDoctor = false
                }
            };
            _log.Error(loginReq.ToJson());
            
            await _client.SendAsync(loginReq);
        }
        
       
        private async Task AskForLoginAsync()
        {
            _log.Information("Hello Client!");
            _log.Information("Wat is uw telefoonnummer? ");
            _username = Console.ReadLine();
            _log.Information("Wat is uw wachtwoord? ");
            _password = Console.ReadLine();

            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                Data = new LoginPacketRequest()
                {
                    UserName = _username,
                    Password = _password,
                    IsDoctor = false
                }
            };

            await _client.SendAsync(loginReq);
        }

        //this methode will get the right methode that will be used for the response from the server
        public void HandleData(DataPacket packet)
        {
            //Checks if the OppCode (OperationCode) does exist.
            if (_callbacks.TryGetValue(packet.OpperationCode, out var action))
            {
                action.Invoke(packet);
            } else {
                throw new Exception("Function not implemented");
            }
        }
        private void EmergencyStopHandler(DataPacket obj)
        {
            EmergencyStopPacket data = obj.GetData<EmergencyStopPacket>();
            _log.Critical(data.Message);
        }

        private void SetResistanceHandeler(DataPacket obj)
        {
            _vrConnection.SetResistance(obj.GetData<SetResistancePacket>().Resistance);
        }

        //the methode for the disconnect request
        private void DisconnectHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<DisconnectPacketResponse>().Message);
        }
        
        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            Console.WriteLine("Sessie gestopt");
            _sessienRunning = false;
            _vrConnection.Session = false;
        }

        //the methode for the session start request
        public void SessionStartHandler(DataPacket obj)
        {
            _sessienRunning = true;
            _vrConnection.Session = true;
            new Thread(SendBikeDataAsync ).Start();
        }
        
        private void SendBikeDataAsync()
        {
            //if the patient started the sessie the while-loop will be looped till it be false (stop-session)
            while (_sessienRunning)
            {
                BikeData bikedata = _vrConnection.GetBikeData();
                HeartData hearthdata = _vrConnection.GetHearthData();
                var req = new DataPacket<BikeDataPacket>
                {
                    OpperationCode = OperationCodes.Bikedata,

                    Data = new BikeDataPacket() 
                    {
                        SessionId = _sessionId,
                        Speed = bikedata.Speed,
                        Distance = bikedata.Distance,
                        HeartRate = hearthdata.HeartRate,
                        Elapsed = bikedata.TotalElapsed,
                        DeviceType = bikedata.DeviceType.ToString(),
                        Id = bikedata.Id
                    }
                };
                
                _log.Information("sending bike data to server");
                 _client.SendAsync(req);
                Thread.Sleep(1000);
            }
        }

        //the methode for printing out the received message and sending it to the VR Engine
        private async void ChatHandlerAsync(DataPacket packetData)
        {
             string messageReceived =
                $"{packetData.GetData<ChatPacketResponse>().SenderName}: {packetData.GetData<ChatPacketResponse>().Message}";
            _log.Information(messageReceived);
            
            ObservableCollection<string> chats = new ObservableCollection<string>();
            foreach (var message in p.Messages)
            {
                chats.Add(message);
            }
            chats.Add($"{packetData.GetData<ChatPacketResponse>().SenderId}: {packetData.GetData<ChatPacketResponse>().Message}");
            p.Messages = chats;

         
            try
            {
                await _vrConnection.Engine.SendTextToChatPannel(messageReceived);
            }
            catch (Exception e)
            {
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
         
            int statusCode = (int)packetData.GetData<LoginPacketResponse>().StatusCode;
            if (statusCode.Equals(200))
            {
                userId = packetData.GetData<LoginPacketResponse>().UserId;
                _log.Information($"Succesfully logged in to the user: {userId}; {_password}; {packetData.GetData<LoginPacketResponse>().UserName}.");
                _loggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().StatusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().Message);
            }
        }
        
        public bool GetLoggedIn()
        {
            return _loggedIn;
        }
    }
   
}