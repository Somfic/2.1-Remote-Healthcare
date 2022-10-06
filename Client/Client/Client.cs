using System.Net.Cache;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

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

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };

            await _client.ConnectAsync("127.0.0.1", 15243);
            
            AskForLoginAsync();

            while (true)
            {
                _log.Information("Voer een command in om naar de server te sturen: \r\n" +
                                  "[BERICHT] [NOODSTOP]");
                string newChatMessage = Console.ReadLine();

                //if the user isn't logged in, the user cant send any command to the server
                if (_loggedIn)
                {
                    if (newChatMessage.ToLower().Equals("bericht"))
                    {
                        _log.Information("Voer uw bericht in: ");
                        newChatMessage = Console.ReadLine();

                        var req = new DataPacket<ChatPacketRequest>
                        {
                            OpperationCode = OperationCodes.CHAT,
                            data = new ChatPacketRequest()
                            {
                                message = newChatMessage
                            }
                        };
    
                        await _client.SendAsync(req);
                    }
                    else if (newChatMessage.ToLower().Equals("noodstop"))
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

        private async void AskForLoginAsync()
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

        private async void RequestDoctorIdAsync()       //TODO, FIX THE SPAMMING TO SERVER IF ENABLED.
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
        
        private void DisconnectHandler(DataPacket obj)
        {
            throw new NotImplementedException();
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
            _log.Information(packetData.GetData<ChatPacketResponse>().message);
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
                // RequestDoctorIdAsync();
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