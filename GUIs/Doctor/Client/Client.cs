using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Client
{
    public class Client
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private static Log _log = new(typeof(Client));

        private static byte[] dataBuffer;
        private static byte[] lengthBytes = new byte[4];

        private static string password;
        private static string username;
        private static bool loggedIn;


        private static Dictionary<string, Action<DataPacket>> functions;


        public Client()
        {
            loggedIn = false;
            functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            functions.Add("login", LoginFeature);
            functions.Add("chat", ChatHandler);
            functions.Add("session start", SessionStartHandler);
            functions.Add("session stop", SessionStopHandler);

            Console.WriteLine("Hello Client!");
            Console.WriteLine("Wat is uw telefoonnummer? ");
            username = Console.ReadLine();
            Console.WriteLine("Wat is uw wachtwoord? ");
            password = Console.ReadLine();

            client = new TcpClient();
            client.BeginConnect("localhost", 15243, new AsyncCallback(OnConnectionMade), null);

            while (true)
            {
                Console.WriteLine("Voer een command in om naar de server te sturen: ");
                string newChatMessage = Console.ReadLine();

                //if the user isn't logged in, the user cant send any command to the server
                if (loggedIn)
                {
                    if (newChatMessage.Equals("chat"))
                    {
                        Console.WriteLine("Voer uw bericht in: ");
                        newChatMessage = Console.ReadLine();

                        DataPacket<ChatPacketRequest> req = new DataPacket<ChatPacketRequest>
                        {
                            OpperationCode = OperationCodes.CHAT,
                            data = new ChatPacketRequest()
                            {
                                message = newChatMessage
                            }
                        };

                        SendData(req);
                    }
                    else if (newChatMessage.Equals("session start"))
                    {
                        DataPacket<SessionStartPacketRequest> req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };

                        SendData(req);
                    }
                    else if (newChatMessage.Equals("session stop"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };

                        SendData(req);
                    }
                    else
                    {
                        Console.WriteLine("in de else bij de client if else elsif statements!");
                    }
                }
                else
                {
                    Console.WriteLine("Je bent nog niet ingelogd");
                }
            }
        }

        //This methode will be enterd if the user has made an TCP-connection
        private static void OnConnectionMade(IAsyncResult ar)
        {
            stream = client.GetStream();

            //Triggers the OnLengthBytesReceived methode
            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);

            //Sends an login request to the server
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    username = username,
                    password = password
                }
            };

            SendData(loginReq);
        }

        //calculates the lenght for the receiveds bytes
        private static void OnLengthBytesReceived(IAsyncResult ar)
        {
            dataBuffer = new byte[BitConverter.ToInt32(lengthBytes, 0)];

            //Triggers the OnDataReceived methode
            stream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnDataReceived, null);
        }

        //this methode receives all the requests from the server and will activate the right methode
        private static void OnDataReceived(IAsyncResult ar)
        {
            stream.EndRead(ar);

            //converts the dataBuffer to an JObject
            string data = Encoding.UTF8.GetString(dataBuffer);

            DataPacket dataPacket = JsonConvert.DeserializeObject<DataPacket>(data);

            //this methode detemines which methode will be called
            handleData(dataPacket);

            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);
        }

        //This methode used to send an request from the Client to the server
        //The parameter is an JsonFile object
        public static void SendData(DAbstract packet)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(packet.ToJson());

            var lengthh = BitConverter.GetBytes(dataBytes.Length);

            stream.Write(lengthh, 0, lengthh.Length);
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Flush();
        }

        //this methode will get the right methode that will be used for the response from the server
        private static void handleData(DataPacket packet)
        {
            Action<DataPacket> action;

            //Checks if the OppCode (OperationCode) does exist.
            if (functions.TryGetValue(packet.OpperationCode, out action))
            {
                action.Invoke(packet);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        }

        //the methode for the session stop request
        private static void SessionStopHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the session start request
        private static void SessionStartHandler(DataPacket obj)
        {
            Console.WriteLine(obj.GetData<SessionStartPacketResponse>().message);
        }

        //the methode for the send chat request
        private static void ChatHandler(DataPacket packetData)
        {
            Console.WriteLine(packetData.GetData<ChatPacketResponse>().message);
        }

        //the methode for the login request
        private static void LoginFeature(DataPacket packetData)
        {
            int status_code = (int)packetData.GetData<LoginPacketResponse>().statusCode;
            if (status_code.Equals(200))
            {
                Console.WriteLine("Logged in!");
                loggedIn = client.Connected;
            }
            else
            {
                Console.WriteLine(packetData.GetData<LoginPacketResponse>().statusCode);
                Console.WriteLine(packetData.GetData<LoginPacketResponse>().message);
            }
        }
    }
}