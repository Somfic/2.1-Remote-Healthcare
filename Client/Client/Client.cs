using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;

namespace RemoteHealthcare.Client {
    public class Client
    {
        private static TcpClient client;
        private static NetworkStream stream;

        private static byte[] dataBuffer;
        private static byte[] lengthBytes = new byte[4];
        
        private static string password;
        private static string username;
        private static bool loggedIn = false;
        
        private static Dictionary<string, Action<JObject>> functions;

        public Client() {
            Main();
        }

        static void Main()
        {
            functions = new Dictionary<string, Action<JObject>>();
            
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
                Console.WriteLine("Voer een commandin om naar de server te sturen: ");
                
                string newChatMessage = Console.ReadLine();
                //if the user isn't logged in, the user cant send any command to the server
                if (loggedIn)
                {
                    if (newChatMessage.Equals("chat")) {
                        Console.WriteLine("Voer uw bericht in: ");
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

                    }else if (newChatMessage.Equals("session start")) {

                        JsonFile req = new JsonFile {
                            StatusCode = (int)StatusCodes.OK,
                            OppCode = OperationCodes.SESSION_START,
                            Username = username,
                            Password = password
                        };
                        SendData(req);
                    }else if (newChatMessage.Equals("session stop")) {

                        JsonFile req = new JsonFile {
                            StatusCode = (int)StatusCodes.OK,
                            OppCode = OperationCodes.SESSION_STOP,
                            Username = username,
                            Password = password
                        };

                        SendData(req);
                    }else {
                        Console.WriteLine("in de else bij de client if else elsif statements!");
                    }
                }
                else {
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
            JsonFile loginReq = new JsonFile
            {
                StatusCode = (int)StatusCodes.OK,
                OppCode = OperationCodes.LOGIN,
                Username = username,
                Password = password
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
            JObject data = JObject.Parse(Encoding.UTF8.GetString(dataBuffer));

            //this methode detemines which methode will be called
            handleData(data);

            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);
        }

        //This methode used to send an request from the Client to the server
        //The parameter is an JsonFile object
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

        //this methode will get the right methode that will be used for the response from the server
        private static void handleData(JObject packetData)
        {
            Action<JObject> action;
            
            //Checks if the OppCode (OperationCode) does exist.
            if (functions.TryGetValue(packetData["OppCode"].ToString(), out action))
            {
                action.Invoke(packetData);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        }
        
        //the methode for the session stop request
        private static void SessionStopHandler(JObject obj)
        {
            Console.WriteLine(obj["Data"]["ChatMessage"].ToString());
        }

        //the methode for the session start request
        private static void SessionStartHandler(JObject obj)
        {
            Console.WriteLine(obj["Data"]["ChatMessage"].ToString());
        }

        //the methode for the send chat request
        private static void ChatHandler(JObject packetData)
        {
            Console.WriteLine($"Chat ontvangen: '{packetData["Data"]["ChatMessage"]}'");
        }
        
        //the methode for the login request
        private static void LoginFeature(JObject packetData)
        {
            if (packetData.Value<int>("StatusCode").Equals(200)) {
                Console.WriteLine("Logged in!");
                loggedIn = true;
            } else {
                Console.WriteLine(packetData.Value<string>("StatusCode"));
                Console.WriteLine(packetData["Data"]["ChatMessage"]);
            }
        }
    }
}