using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.GUIs.Doctor.Client
{
    public class Client
    {
        private SocketClient _client = new(true);
        private List<string> _connected;
        private Log _log = new(typeof(Client));

        private bool _loggedIn;
        private string _username;
        private string _password;
        private string userId;

        private Dictionary<string, Action<DataPacket>> _functions = new();

        public async Task RunAsync()
        {
            _loggedIn = false;

            //Adds for each key an callback methode in the dictionary 
            _functions.Add("login", LoginFeature);
            _functions.Add("users", RequestConnectionsFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("emergency stop", EmergencyStopHandler);

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };

            await _client.ConnectAsync("127.0.0.1", 15243);

            AskForLoginAsync();

            while (true)
            {
                //if the user isn't logged in, the user cant send any command to the server
                if (_loggedIn)
                {
                    _log.Information("Voer een command in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [START SESSIE] [STOP SESSIE] [NOODSTOP]");
                    string userCommand = Console.ReadLine();

                    if (userCommand.ToLower().Equals("bericht"))
                    {
                        SendChatAsync();
                    }
                    else if (userCommand.ToLower().Equals("start") && userCommand.ToLower().Equals("sessie"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };

                        await _client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Contains(("stop")) && userCommand.ToLower().Contains("Sessie"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };

                        await _client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Equals("noodstop"))
                    {
                        var req = new DataPacket<EmergencyStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.EMERGENCY_STOP,
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

        private async void SendChatAsync()
        {
            await requestClients();
            while (_connected == null)
            {
            }
            
            _log.Information("Voer uw bericht in: ");
            String chatInput = Console.ReadLine();
            string savedConnections = "";
            _connected.ForEach(c => savedConnections+= c + "; ");
            _log.Information($"Voor welk accountnummer is dit bedoeld: [{savedConnections}]");
            String target = Console.ReadLine();

            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = userId,
                    receiverId = target,
                    message = chatInput
                }
            };
            
            _log.Debug(req.ToJson());
            
            await _client.SendAsync(req);
        }

        private async Task requestClients()
        {
            var req = new DataPacket<ConnectedClientsPacketRequest>
            {
                OpperationCode = OperationCodes.USERS
            };

            await _client.SendAsync(req);
        }

        private async void AskForLoginAsync()
        {
            _log.Information("Hallo Dokter!");
            _log.Information("Wat is uw loginId? ");
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
                    isDoctor = true
                }
            };

            await _client.SendAsync(loginReq);
        }

        //this methode will get the right methode that will be used for the response from the server
        private void HandleData(DataPacket packet)
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

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the emergency stop request
        //TODO 
        private void EmergencyStopHandler(DataPacket obj)
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
            _log.Information(packetData.GetData<ChatPacketResponse>().message);
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().statusCode).Equals(200))
            {
                _log.Critical($"Size: {packetData.GetData<ConnectedClientsPacketResponse>().connectedIds.Count()} " +
                              $"id's: {packetData.GetData<ConnectedClientsPacketResponse>().connectedIds}");
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().connectedIds.Split(";").ToList();
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().statusCode).Equals(200))
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