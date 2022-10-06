﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;
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

        public string password { get; set; }
        public string username { get; set; }
        public bool loggedIn { get; set; }
        private string _userId;

        private Dictionary<string, Action<DataPacket>> _functions = new();

        public async Task RunAsync()
        {
            loggedIn = false;
            _functions = new Dictionary<string, Action<DataPacket>>();

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
                if (loggedIn)
                {
                    _log.Information("Voer een commando in om naar de server te sturen: \r\n" +
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
            /* This is a while loop that will do nothing until connected is filled */
            while (_connected == null)
            {
            }

            string savedConnections = "";
            _connected.ForEach(c => savedConnections += c + "; ");
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
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _userId,
                    receiverId = target,
                    message = chatInput
                }
            };

            await _client.SendAsync(req);
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
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    username = username,
                    password = password,
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
            _log.Information(
                $"{packetData.GetData<ChatPacketResponse>().senderId}: {packetData.GetData<ChatPacketResponse>().message}");
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            _log.Debug(packetData.ToJson());
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().statusCode).Equals(200))
            {
                _connected.Clear();
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().connectedIds.Split(";").ToList();
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().statusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().userId;
                _log.Information($"Succesfully logged in to the user: {username}; {password}; {_userId}.");
                loggedIn = true;
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