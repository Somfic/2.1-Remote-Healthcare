using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.GUIs.Doctor.Client
{
    public class Client
    {
        public SocketClient Client { get; set; } = new(true);
        private List<string> _connected;
        private Log _log = new(typeof(Client));

        public string Password { get; set; }
        public string Username { get; set; }
        public bool LoggedIn { get; set; }
        private string _userId;

        private Dictionary<string, Action<DataPacket>> _functions = new();

        public Client()
        {
            LoggedIn = false;
            _functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _functions.Add("login", LoginFeature);
            _functions.Add("users", RequestConnectionsFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("emergency stop", EmergencyStopHandler);

            Client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };
        }

        public async Task RunAsync()
        {
            while (true)
            {
                //if the user isn't logged in, the user cant send any command to the server
                if (LoggedIn)
                {
                    _log.Information("Voer een commando in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [START SESSIE] [STOP SESSIE] [NOODSTOP]");
                    string userCommand = "";

                    if (userCommand.ToLower().Equals("bericht"))
                    {
                        SendChatAsync();
                    }
                    else if (userCommand.ToLower().Equals("start") && userCommand.ToLower().Equals("sessie"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SessionStart,
                        };

                        await Client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Contains(("stop")) && userCommand.ToLower().Contains("Sessie"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SessionStop,
                        };

                        await Client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Equals("noodstop"))
                    {
                        var req = new DataPacket<EmergencyStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.EmergencyStop,
                        };

                        await Client.SendAsync(req);
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
            await RequestClients();
            
            /* This is a while loop that will do nothing until connected is filled */
            while (_connected.Count == 0)
            {
                _log.Debug("Loading...");
            }
            _log.Information("escaped loading");
            string savedConnections = " ";
            foreach (string id in _connected)
            {
                savedConnections += id + "; ";
                _log.Debug($"{id} has been added, saved connections is now: [{savedConnections}]");
            }

            string? target = 0000 + "";

            /* This is a while loop that will keep asking for a target until the target is in the list of connected
            clients. */
            while (!_connected.Contains(target) && !target.Contains(";"))
            {
                _log.Information($"Voor welk accountnummer is dit bedoeld? Voor meerdere accountnummers tegelijk, " +
                                 $"gebruik een ; tussen de nummers. Kies uit de volgende beschikbare " +
                                 $"accountnummers: \t[{savedConnections}]");
                target = Console.ReadLine();

                //breaks the while-loop if all targets are correct.
                if (CheckTargets(target.Split(";").ToList(), _connected))
                    break;
            }

            _log.Information("Voer uw bericht in: ");
            String chatInput = Console.ReadLine();

            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.Chat,
                Data = new ChatPacketRequest()
                {
                    SenderId = _userId,
                    ReceiverId = target,
                    Message = chatInput
                }
            };

            await Client.SendAsync(req);
        }

        /// <summary>
        /// It checks if all the targets are in the connections list
        /// </summary>
        /// <param name="targets">A list of strings that represent the targets you want to check for.</param>
        /// <param name="connections">A list of all the connections that are currently active.</param>
        /// <returns>
        /// True if all targets are in connections list, false if not so.
        /// </returns>
        private bool CheckTargets(List<string> targets, List<string> connections)
        {
            foreach (string target in targets)
            {
                if (!connections.Contains(target))
                    return false;
            }

            return true;
        }

        private async Task RequestClients()
        {
            if (_connected != null)
                _connected.Clear();
            var req = new DataPacket<ConnectedClientsPacketRequest>
            {
                OpperationCode = OperationCodes.Users
            };

            await Client.SendAsync(req);
        }

        public async Task AskForLoginAsync()
        {
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                Data = new LoginPacketRequest()
                {
                    Username = Username,
                    Password = Password,
                    IsDoctor = true
                }
            };

            _log.Debug(loginReq.ToJson());
            
            await Client.SendAsync(loginReq);
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

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().Message);
        }

        //the methode for the emergency stop request
        private void EmergencyStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().Message);
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStartPacketResponse>().Message);
        }

        //the methode for the send chat request
        private void ChatHandler(DataPacket packetData)
        {
            _log.Information(
                $"Incomming message: {packetData.GetData<ChatPacketResponse>().SenderId}: {packetData.GetData<ChatPacketResponse>().Message}");
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            _log.Debug(packetData.ToJson());
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().StatusCode).Equals(200))
            {
                // _log.Debug(_connected.Count.ToString());
                // _connected.RemoveRange(0, _connected.Count - 1);
                // _log.Debug(_connected.Count.ToString());
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().ConnectedIds.Split(";").ToList();
                _log.Critical(_connected.Count.ToString());
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().StatusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().UserId;
                _log.Information($"Succesfully logged in to the user: {Username}; {Password}; {_userId}.");
                LoggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().StatusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().Message);
            }
        }
    }
}