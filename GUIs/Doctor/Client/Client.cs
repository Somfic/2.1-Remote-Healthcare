﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.Client
{
    public class Client
    {
        private SocketClient client = new(true);
        private Log _log = new(typeof(Client));

        private string password;
        private string username;
        private bool _loggedIn;

        private static Dictionary<string, Action<DataPacket>> functions;

        public async Task RunAsync()
        {
            _loggedIn = false;
            functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            functions.Add("login", LoginFeature);
            functions.Add("chat", ChatHandler);
            functions.Add("session start", SessionStartHandler);
            functions.Add("session stop", SessionStopHandler);

            client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };

            await client.ConnectAsync("127.0.0.1", 15243);
            
            AskForLoginAsync();

            while (true)
            {
                //if the user isn't logged in, the user cant send any command to the server
                if (_loggedIn)
                {
                    _log.Information("Voer een command in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [START SESSIE] [STOP SESSIE] [NOODSTOP]");
                    string newChatMessage = Console.ReadLine();
                    
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

                        await client.SendAsync(req);
                    }
                    else if (newChatMessage.ToLower().Equals("start sessie"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };

                        await client.SendAsync(req);
                    }
                    else if (newChatMessage.ToLower().Equals("stop sessie"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };

                        await client.SendAsync(req);
                    }
                    else if (newChatMessage.ToLower().Equals("noodstop"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.EMERGENCY_STOP,
                        };

                        await client.SendAsync(req);
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
            
            _log.Information("Hallo Dokter!");
            _log.Information("Wat is uw loginId? ");
            username = Console.ReadLine();
            _log.Information("Wat is uw wachtwoord? ");
            password = Console.ReadLine();

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

            _log.Debug(loginReq.ToJson());

            await client.SendAsync(loginReq);
        }

        //this methode will get the right methode that will be used for the response from the server
        private void HandleData(DataPacket packet)
        {
            //Checks if the OppCode (OperationCode) does exist.
            if (functions.TryGetValue(packet.OpperationCode, out var action))
            {
                action.Invoke(packet);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        } //the methode for the session stop request

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
            int statusCode = (int)packetData.GetData<LoginPacketResponse>().statusCode;

            if (statusCode.Equals(200))
            {
                _log.Information("Logged in!");
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