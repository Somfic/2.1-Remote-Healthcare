using System.Net.Cache;
using Newtonsoft.Json;
using RemoteHealthcare.Client.Data;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Client;
using NetworkEngine.Socket;

namespace RemoteHealthcare.Client.Client
{
    public class Client
    {
        private SocketClient _client = new(true);
        private Log _log = new(typeof(Client));

        private bool _loggedIn;
        private string _password;
        private string _username;
        private string userId;
        private string doctorId;

        private VrConnection _vrConnection;

        public Client(VrConnection connection)
        {
            _vrConnection = connection;
        }
        
        private Dictionary<string, Action<DataPacket>> _functions;

        public async Task RunAsync()
        {
            _loggedIn = false;
            _functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _functions.Add("login", LoginFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("disconnect", DisconnectHandler);

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };

            await _client.ConnectAsync("127.0.0.1", 15243);

            await AskForLoginAsync();
            new Thread(e => SendBikeDataAsync()).Start();
            while (true)
            {
                //if the user isn't logged in, the user cant send any command to the server
                if (_loggedIn)
                {
                    _log.Information("Voer een commando in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [NOODSTOP] [VERBREEK VERBINDING]");
                    string command = Console.ReadLine();
                    
                    if (command.ToLower().Equals("bericht"))
                    {
                        _log.Information("Voer uw bericht in: ");
                        string ChatMessage = Console.ReadLine();

                        var req = new DataPacket<ChatPacketRequest>
                        {
                            OpperationCode = OperationCodes.CHAT,
                            
                            data = new ChatPacketRequest()
                            {
                                senderId = userId,
                                receiverId = null,
                                message = ChatMessage
                            }
                        };

                        await _client.SendAsync(req);
                    }
                    else if (command.ToLower().Equals("noodstop"))
                    {
                        var req = new DataPacket<EmergencyStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.EMERGENCY_STOP,
                        };

                    }else if (command.ToLower().Contains("verbreek") && command.ToLower().Contains("verbinding")) {

                        var req = new DataPacket<DisconnectPacketRequest> {
                            OpperationCode = OperationCodes.DISCONNECT
                        };

                        await _client.SendAsync(req);
                    }
                    else
                    {
                        _log.Warning("Het commando dat u heeft ingevoerd is incorrect.");
                    }
                }
            }
        }

        private async void SendBikeDataAsync()
        {
            while (true)
            {
                BikeData bikedata = _vrConnection.getBikeData();
                HeartData hearthdata = _vrConnection.getHearthData();
                var req = new DataPacket<BikeDataPacket>
                {
                    OpperationCode = OperationCodes.BIKEDATA,

                    data = new BikeDataPacket()
                    {
                        speed = bikedata.Speed,
                        distance = bikedata.Distance,
                        heartRate = hearthdata.HeartRate,
                        elapsed = bikedata.TotalElapsed,
                        deviceType = bikedata.DeviceType.ToString(),
                        id = bikedata.Id

                    }
                };
                _log.Information("sending bike data to server");
                await _client.SendAsync(req);
                await Task.Delay(1000);
            }
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
            if (_functions.TryGetValue(packet.OpperationCode, out var action))
            {
                action.Invoke(packet);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        }

        private void SetResistanceHandeler(DataPacket obj)
        {
            _vrConnection.setResistance(obj.GetData<SetResistancePacket>().resistance);
        }

        private void DisconnectHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<DisconnectPacketResponse>().message);
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStartPacketResponse>().message);
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
    }
}