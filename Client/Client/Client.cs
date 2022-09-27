using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;

namespace RemoteHealthcare.Client
{

    public class Client
    {
        private static string password;
        private static TcpClient client;
        private static NetworkStream stream;
        private static string totalBuffer;
        private static string username;

        private static byte[] dataBuffer;
        private static byte[] lengthBytes = new byte[4];

        private static bool loggedIn = false;
        private static Dictionary<string, Action<JObject>> functions;

        public Client()
        {
            Main();
        }

        static void Main()
        {
            functions = new Dictionary<string, Action<JObject>>();
            functions.Add("login", LoginFeature);
            functions.Add("chat", ChatHandler);
            functions.Add("session start", SessionStartHandler);
            functions.Add("session stop", SessionStopHandler);

            Console.WriteLine("Hello Client!");
            Console.WriteLine("Wat is je gebruikersnaam? ");
            username = Console.ReadLine();
            Console.WriteLine("Wat is je wachtwoord? ");
            password = Console.ReadLine();

            client = new TcpClient();
            client.BeginConnect("localhost", 15243, new AsyncCallback(OnConnectionMade), null);

            while (true)
            {
                Console.WriteLine("Voer een chat bericht om naar de server te sturen:");
                string newChatMessage = Console.ReadLine();
                if (loggedIn)
                {
                    if (newChatMessage.Equals("chat"))
                    {
                        newChatMessage = Console.ReadLine();

                        JsonFile req = new JsonFile
                        {
                            StatusCode = (int)StatusCodes.OK,
                            OppCode = OperationCodes.CHAT,
                            Username = username,
                            Password = password,
                            Data = new JsonData
                            {
                                ChatMessage = newChatMessage
                            }
                        };

                        SendData(req);

                    }
                    else if (newChatMessage.Equals("session start"))
                    {

                        JsonFile req = new JsonFile
                        {
                            StatusCode = (int)StatusCodes.OK,
                            OppCode = OperationCodes.SESSION_START,
                            Username = username,
                            Password = password
                        };

                        SendData(req);


                    }
                    else if (newChatMessage.Equals("session stop"))
                    {

                        JsonFile req = new JsonFile
                        {
                            StatusCode = (int)StatusCodes.OK,
                            OppCode = OperationCodes.SESSION_STOP,
                            Username = username,
                            Password = password
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

        private static void SessionStopHandler(JObject obj)
        {
            Console.WriteLine(obj["Data"]["ChatMessage"].ToString());
        }

        private static void SessionStartHandler(JObject obj)
        {
            Console.WriteLine(obj["Data"]["ChatMessage"].ToString());
        }


        private static void ChatHandler(JObject packetData)
        {
            Console.WriteLine($"Chat ontvangen: '{packetData["Data"]["ChatMessage"].ToString()}'");
        }

        private static void LoginFeature(JObject packetData)
        {
            if (packetData.Value<int>("StatusCode").Equals(200))
            {
                Console.WriteLine("Logged in!");
                loggedIn = true;
            }
            else
            {
                Console.WriteLine(packetData.Value<string>("StatusCode")+" "+packetData.Value<string>("Data"));
            }
        }

        private static void OnConnectionMade(IAsyncResult ar)
        {
            stream = client.GetStream();
            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);

            JsonFile loginReq = new JsonFile
            {
                StatusCode = (int)StatusCodes.OK,
                OppCode = OperationCodes.LOGIN,
                Username = username,
                Password = password
            };

            SendData(loginReq);
        }

        private static void OnLengthBytesReceived(IAsyncResult ar)
        {
            dataBuffer = new byte[BitConverter.ToInt32(lengthBytes, 0)];

            stream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnDataReceived, null);
        }


        private static void OnDataReceived(IAsyncResult ar)
        {
            stream.EndRead(ar);

            JObject data = JObject.Parse(Encoding.UTF8.GetString(dataBuffer));

            handleData(data);

            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);
        }


        public static void SendData(JsonFile jsonFile)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(
                jsonFile,
                Formatting.Indented,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));

            var lengthh = BitConverter.GetBytes(dataBytes.Length);

            stream.Write(lengthh, 0, lengthh.Length);
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Flush();
        }

        private static void handleData(JObject packetData)
        {
            Action<JObject> action;

            if (functions.TryGetValue(packetData["OppCode"].ToString(), out action))
            {
                action.Invoke(packetData);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        }
    }
}