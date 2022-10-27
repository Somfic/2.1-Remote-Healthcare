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
using RemoteHealthcare.GUIs.Patient.ViewModels;
using System.Collections;
using System.Windows;
using RemoteHealthcare.Common.Data;

namespace RemoteHealthcare.GUIs.Patient.Client
{
   public class Client
    {
        public SocketClient _client = new(true);
        private Log _log = new(typeof(Client));

        public bool LoggedIn;
        public string Password;
        public string Username;
        public string UserId;
        private readonly string _sessionId;
        public PatientHomepageViewModel P;
        private bool _sessionRunning;

        private readonly Dictionary<string, Action<DataPacket>> _callbacks;
        public VrConnection VrConnection;
        public Client()
        {
            LoggedIn = false;
            _callbacks = new Dictionary<string, Action<DataPacket>>
            {
                { "login", LoginFeature },
                { "chat", ChatHandlerAsync },
                { "session start", SessionStartHandler },
                { "session stop", SessionStopHandler },
                { "disconnect", DisconnectHandler },
                { "set resistance", SetResistanceHandler },
                { "emergency stop", EmergencyStopHandler }
            };

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
                data = new LoginPacketRequest()
                {
                    userName = Username,
                    password = Password,
                    isDoctor = false
                }
            };
            _log.Error(loginReq.ToJson());
            
            await _client.SendAsync(loginReq);
        }
        
        /// <summary>
        /// If the packet's operation code is in the dictionary, invoke the function associated with it
        /// </summary>
        /// <param name="DataPacket">This is the data packet that was received from the server.</param>
        private void HandleData(DataPacket packet)
        {
            if (_callbacks.TryGetValue(packet.OpperationCode, out var action))
            {
                action.Invoke(packet);
            } else {
                throw new Exception("Function not implemented");
            }
        }
        
        /// <summary>
        /// This function is called when the server sends an emergency stop packet. It logs the message, stops the session,
        /// and calls the emergency stop function in the robot class
        /// </summary>
        /// <param name="DataPacket">The data packet that was received.</param>
        private void EmergencyStopHandler(DataPacket obj)
        {
            EmergencyStopPacket data = obj.GetData<EmergencyStopPacket>();
            _log.Critical(data.message);
            SessionStopHandler(obj);
            MessageBox.Show("emergency stop!");
        }

        /// <summary>
        /// This function is called when the server sends a SetResistancePacket to the client. It calls the
        /// VrConnection.setResistance function, which sets the resistance of the bike
        /// </summary>
        /// <param name="DataPacket">The data packet that was received from the client.</param>
        private void SetResistanceHandler(DataPacket obj)
        {
            VrConnection.SetResistance(obj.GetData<SetResistancePacket>().resistance);
        }
        
        /// <summary>
        /// It prints out the message from the server.
        /// </summary>
        /// <param name="DataPacket">The packet that was received.</param>
        private void DisconnectHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<DisconnectPacketResponse>().message);
        }
        
        /// <summary>
        /// When the server sends a message that the session has stopped, the session is stopped and the bike speed is set
        /// to 0
        /// </summary>
        /// <param name="DataPacket">This is the object that is sent from the server. It contains the following
        /// parameters:</param>
        private void SessionStopHandler(DataPacket obj)
        {
            Console.WriteLine("Sessie gestopt");
            _sessionRunning = false;
            VrConnection.Session = false;
            VrConnection.Engine.ChangeBikeSpeed(0);
        }
        
        
        /// <summary>
        /// The function is called when the client receives a message from the server that the session has started. It sets
        /// the sessionRunning variable to true, which is used to start the thread that sends the bike data to the server
        /// </summary>
        /// <param name="DataPacket">This is the data packet that is sent from the client to the server.</param>
        private void SessionStartHandler(DataPacket obj)
        {
            _sessionRunning = true;
            VrConnection.Session = true;
            new Thread(SendBikeDataAsync ).Start();
        }
        
        /// <summary>
        /// This function is called when the user starts a session. It will send the bike data to the server every second
        /// </summary>
        private void SendBikeDataAsync()
        {
            while (_sessionRunning)
            {
                BikeData bikeData = VrConnection.GetBikeData();
                HeartData heartData = VrConnection.GetHearthData();
                var req = new DataPacket<BikeDataPacket>
                {
                    OpperationCode = OperationCodes.Bikedata,

                    data = new BikeDataPacket() 
                    {
                        SessionId = _sessionId,
                        speed = bikeData.Speed,
                        distance = bikeData.Distance,
                        heartRate = heartData.HeartRate,
                        elapsed = bikeData.TotalElapsed,
                        deviceType = bikeData.DeviceType.ToString(),
                        id = bikeData.Id
                    }
                };
                
                _log.Information("sending bike data to server");
                 _client.SendAsync(req);
                Thread.Sleep(1000);
            }
        }
        
        /// <summary>
        /// It takes the message received from the server, adds it to the chat list, and sends it to the chat panel
        /// </summary>
        /// <param name="DataPacket">This is the packet that is received from the server.</param>
        private async void ChatHandlerAsync(DataPacket packetData)
        {
             string messageReceived =
                $"{packetData.GetData<ChatPacketResponse>().senderName}: {packetData.GetData<ChatPacketResponse>().message}";
            _log.Information(messageReceived);
            
            ObservableCollection<string> chats = new ObservableCollection<string>();
            foreach (var message in P.Messages)
            {
                chats.Add(message);
            }
            chats.Add($"{packetData.GetData<ChatPacketResponse>().senderId}: {packetData.GetData<ChatPacketResponse>().message}");
            P.Messages = chats;
            
            try
            {
                await VrConnection.Engine.SendTextToChatPannel(messageReceived);
            }
            catch (Exception e)
            {
            }
        }
        
        /// <summary>
        /// This function is called when the server responds to the login request
        /// </summary>
        /// <param name="DataPacket">This is the packet that is received from the server.</param>
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Response: {packetData.ToJson()}");
         
            int statusCode = (int)packetData.GetData<LoginPacketResponse>().statusCode;
            if (statusCode.Equals(200))
            {
                UserId = packetData.GetData<LoginPacketResponse>().userId;
                Username = packetData.GetData<LoginPacketResponse>().userName;
                _log.Information($"Successfully logged in to the user: {UserId}; {Password}; {packetData.GetData<LoginPacketResponse>().userName}.");
                LoggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().statusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().message);
            }
        }
    }
}