using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteHealthcare.Client.Data;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Client;
using NetworkEngine.Socket;

namespace RemoteHealthcare.GUIs.Patient.Client
{
   public class Client
    {
        public SocketClient _client = new(true);
        private Log _log = new(typeof(Client));
        private string userId;
        public VrConnection _vrConnection;

        public string _password { get; set; }
        public string _username { get; set; }
        public bool _loggedIn;
        private Boolean _sessienRunning = false;
        private string _sessionId;

        private static Dictionary<string, Action<DataPacket>> _callbacks;

        public  Client(VrConnection v)
        {
            _callbacks = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _callbacks.Add(OperationCodes.LOGIN, LoginFeature);
            _callbacks.Add(OperationCodes.CHAT, ChatHandler);
            _callbacks.Add(OperationCodes.SESSION_START, SessionStartHandler);
            _callbacks.Add(OperationCodes.SESSION_STOP, SessionStopHandler);

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
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    username = _username,
                    password = _password,
                    isDoctor = false
                }
            };
            _log.Debug(loginReq.ToJson());
            
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
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    username = _username,
                    password = _password,
                    isDoctor = false
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

        private void SetResistanceHandeler(DataPacket obj)
        {
            _vrConnection.setResistance(obj.GetData<SetResistancePacket>().resistance);
        }

        //the methode for the disconnect request
        private void DisconnectHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<DisconnectPacketResponse>().message);
        }
        
        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            Console.WriteLine("Sessie gestopt");
            _sessienRunning = false;
        }

        //the methode for the session start request
        public async void SessionStartHandler(DataPacket obj)
        {
            _sessienRunning = true;
            
            new Thread(SendBikeDataAsync ).Start();
        }
        
        private void SendBikeDataAsync()
        {
            while (_sessienRunning)
            {
                BikeData bikedata = _vrConnection.getBikeData();
                HeartData hearthdata = _vrConnection.getHearthData();
                var req = new DataPacket<BikeDataPacket>
                {
                    OpperationCode = OperationCodes.BIKEDATA,

                    data = new BikeDataPacket() 
                    {
                        SessionId = _sessionId,
                        speed = bikedata.Speed,
                        distance = bikedata.Distance,
                        heartRate = hearthdata.HeartRate,
                        elapsed = bikedata.TotalElapsed,
                        deviceType = bikedata.DeviceType.ToString(),
                        id = bikedata.Id
                    }
                };
                
                _log.Information("sending bike data to server");
                 _client.SendAsync(req);
                //Task.Delay(1000);
                Thread.Sleep(1000);
            }
        }

        
        //the methode for the send chat request
        private void ChatHandler(DataPacket packetData)
        {
            _log.Information($"{packetData.GetData<ChatPacketResponse>().senderId}: {packetData.GetData<ChatPacketResponse>().message}");
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
         
            int statusCode = (int)packetData.GetData<LoginPacketResponse>().statusCode;
            if (statusCode.Equals(200))
            {
                userId = packetData.GetData<LoginPacketResponse>().userId;
                _log.Information($"Succesfully logged in to the user: {_username}; {_password}; {userId}.");
                _loggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().statusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().message);
                AskForLoginAsync();
            }
        }
        public bool GetLoggedIn()
        {
            return _loggedIn;
        }
    }
   
}