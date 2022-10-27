using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using NetworkEngine.Socket;
using RemoteHealthcare.GUIs.Patient.ViewModels;

namespace RemoteHealthcare.GUIs.Patient.Client
{
    public class Client
    {
        public SocketClient _client = new(true);
        private readonly Log _log = new(typeof(Client));

        public bool LoggedIn;
        public string Password;
        public string Username;
        public string UserId;
        private string _doctorId;
        private readonly string _sessionId;
        public PatientHomepageViewModel p;
        private bool _sessienRunning;

        private readonly Dictionary<string, Action<DataPacket>> _callbacks;
        public VrConnection _vrConnection;

        public Client()
        {
            LoggedIn = false;
            //Adds for each key an callback methode in the dictionary 
            _callbacks = new Dictionary<string, Action<DataPacket>>
            {
                { "login", LoginFeature },
                { "chat", ChatHandlerAsync },
                { "session start", SessionStartHandler },
                { "session stop", SessionStopHandler },
                { "disconnect", DisconnectHandler },
                { "set resitance", SetResistanceHandeler },
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
            var loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                data = new LoginPacketRequest()
                {
                    UserName = Username,
                    Password = Password,
                    IsDoctor = false
                }
            };

            await _client.SendAsync(loginReq);
        }


        private async Task AskForLoginAsync()
        {
            _log.Information("Hello Client!");
            _log.Information("Wat is uw telefoonnummer? ");
            Username = Console.ReadLine();
            _log.Information("Wat is uw wachtwoord? ");
            Password = Console.ReadLine();

            var loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                data = new LoginPacketRequest()
                {
                    UserName = Username,
                    Password = Password,
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
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        }

        private void EmergencyStopHandler(DataPacket obj)
        {
            var data = obj.GetData<EmergencyStopPacket>();
            SessionStopHandler(obj);
            p.emergencyStop();
        }

        private void SetResistanceHandeler(DataPacket obj)
        {
            _vrConnection.setResistance(obj.GetData<SetResistancePacket>().Resistance);
        }

        //the methode for the disconnect request
        private void DisconnectHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<DisconnectPacketResponse>().Message);
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _sessienRunning = false;
            _vrConnection.session = false;
            _vrConnection.Engine.ChangeBikeSpeed(0);


            //_thread.Abort();
        }

        private Thread _thread;

        //the methode for the session start request
        public void SessionStartHandler(DataPacket obj)
        {
            _sessienRunning = true;
            _vrConnection.session = true;
            new Thread(SendBikeDataAsync).Start();
        }

        private void SendBikeDataAsync()
        {
            //if the patient started the sessie the while-loop will be looped till it be false (stop-session)
            while (_sessienRunning)
            {
                var bikedata = _vrConnection.getBikeData();
                var hearthdata = _vrConnection.getHearthData();
                var req = new DataPacket<BikeDataPacket>
                {
                    OpperationCode = OperationCodes.Bikedata,

                    data = new BikeDataPacket()
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

                _client.SendAsync(req);
                Thread.Sleep(1000);
            }
        }

        //the methode for printing out the received message and sending it to the VR Engine
        private async void ChatHandlerAsync(DataPacket packetData)
        {
            var messageReceived =
                $"{packetData.GetData<ChatPacketResponse>().SenderName}: {packetData.GetData<ChatPacketResponse>().Message}";
            _log.Information(messageReceived);

            var chats = new ObservableCollection<string>();
            foreach (var message in p.Messages)
            {
                chats.Add(message);
            }

            chats.Add(
                $"Dr. {packetData.GetData<ChatPacketResponse>().SenderName}: {packetData.GetData<ChatPacketResponse>().Message}");
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

            var statusCode = (int)packetData.GetData<LoginPacketResponse>().StatusCode;
            if (statusCode.Equals(200))
            {
                UserId = packetData.GetData<LoginPacketResponse>().UserId;
                Username = packetData.GetData<LoginPacketResponse>().UserName;
                _log.Information(
                    $"Succesfully logged in to the user: {UserId}; {Password}; {packetData.GetData<LoginPacketResponse>().UserName}.");
                LoggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().StatusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().Message);
            }
        }

        public bool GetLoggedIn()
        {
            return LoggedIn;
        }
    }
}