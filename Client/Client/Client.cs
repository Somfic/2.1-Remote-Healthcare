using System.Net.Sockets;
using System.Text;
using NetworkEngine.Socket;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.Client
{
    public class Client
    {
        private SocketClient _client = new(true);
        private Log _log = new(typeof(Client));

        private string _password;
        private string _username;
        private bool _loggedIn;
        private VrConnection _vrConnection;
        
        private Dictionary<string, Action<DataPacket>> _functions;
        
        public Client(VrConnection vr)
        {
            _vrConnection = vr;
        }

        public async Task RunAsync()
        {
            _loggedIn = false;
            _functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _functions.Add("login", LoginFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("Disconnect", DisconnectHandler);
            
            Console.WriteLine("Hello Client!");
            Console.WriteLine("Wat is uw telefoonnummer? ");
            _username = Console.ReadLine();
            Console.WriteLine("Wat is uw wachtwoord? ");
            _password = Console.ReadLine();

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };

            await _client.ConnectAsync("127.0.0.1", 15243);

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

            while (true)
            {
                Console.WriteLine("Voer een command in om naar de server te sturen: ");
                var newChatMessage = Console.ReadLine();

                //if the user isn't logged in, the user cant send any command to the server
                if (_loggedIn)
                {
                    if (newChatMessage.Equals("chat"))
                    {
                        Console.WriteLine("Voer uw bericht in: ");
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
                    else if (newChatMessage.ToLower().StartsWith("setresistance:"))
                    {
                        int resistance = int.Parse(newChatMessage.Remove(0, 14));
                        _vrConnection.setResistance(resistance);
                    }
                    else if (newChatMessage.Equals("session start"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };

                        await _client.SendAsync(req);
                    }
                    else if (newChatMessage.Equals("session stop"))
                    {
                        var req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };

                       await _client.SendAsync(req);
                    }else if (newChatMessage.Equals("disconnect")) {

                        Console.WriteLine("in de disconnect else if");
                        var req = new DataPacket<SessionStopPacketRequest> {
                            OpperationCode = OperationCodes.DISCONNECT,
                        };

                        await _client.SendAsync(req);
                    }else {
                        Console.WriteLine("in de else bij de client if else elsif statements!");
                    }
                }
                else
                {
                    Console.WriteLine("Je bent nog niet ingelogd");
                }
            }
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

        private void SetResistanceHandeler(DataPacket obj)
        {
            string newChatMessage = obj.GetData<SetResistancePacketResponse>().message;
            var resistance = int.Parse(newChatMessage.Remove(0, 14));
            _vrConnection.setResistance(resistance);
        }

        //the methode for the session stop request
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