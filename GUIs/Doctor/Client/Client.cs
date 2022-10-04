using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.GUIs.Doctor.Client
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

            _log.Information("Hallo Dokter!");
            _log.Information("Wat is uw loginId? ");
            username = Console.ReadLine();
            _log.Information("Wat is uw wachtwoord? ");
            password = Console.ReadLine();

            client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };

            await client.ConnectAsync("127.0.0.1", 15243);

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

            await client.SendAsync(loginReq);

            while (true)
            {
                Console.WriteLine("Voer een command in om naar de server te sturen: ");
                string newChatMessage = Console.ReadLine();

                //if the user isn't logged in, the user cant send any command to the server
                if (_loggedIn)
                {
                    if (newChatMessage.Equals("chat"))
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
                    else if (newChatMessage.Equals("session start"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };

                        await client.SendAsync(req);
                    }
                    else if (newChatMessage.Equals("session stop"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };

                        await client.SendAsync(req);
                    }
                    else
                    {
                        _log.Debug("in de else bij de client if else elsif statements!");
                    }
                }
                else
                {
                    _log.Critical("Je bent nog niet ingelogd");
                }
            }
        }

        //This methode will be enterd if the user has made an TCP-connection
        // private void OnConnectionMade(IAsyncResult ar)
        // {
        //     stream = client.GetStream();
        //
        //     //Triggers the OnLengthBytesReceived methode
        //     stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);
        //
        //     //Sends an login request to the server
        //     DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
        //     {
        //         OpperationCode = OperationCodes.LOGIN,
        //         data = new LoginPacketRequest()
        //         {
        //             username = username,
        //             password = password,
        //             isDoctor = true
        //         }
        //     };
        //
        //     SendData(loginReq);
        // }

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
            Console.WriteLine(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<SessionStartPacketResponse>().message);
        }

        //the methode for the send chat request
        private void ChatHandler(DataPacket packetData)
        {
            Console.WriteLine(packetData.GetData<ChatPacketResponse>().message);
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            int statusCode = (int)packetData.GetData<LoginPacketResponse>().statusCode;

            if (statusCode.Equals(200))
            {
                Console.WriteLine("Logged in!");
                _loggedIn = true;
            }
            else
            {
                Console.WriteLine(packetData.GetData<LoginPacketResponse>().statusCode);
                Console.WriteLine(packetData.GetData<LoginPacketResponse>().message);
            }
        }
    }
}