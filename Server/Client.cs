using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;

namespace RemoteHealthcare.CentralServer
{
    internal class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private string totalBuffer = "";

        private byte[] dataBuffer;
        private readonly byte[] lengthBytes = new byte[4];

        public string UserName { get; set; }
        private Dictionary<string, Action<JObject>> functions;

        public Client(TcpClient tcpClient)
        {
            this.functions = new Dictionary<string, Action<JObject>>();
            this.functions.Add("login", this.LoginFeature);
            this.functions.Add("chat", this.ChatHandler);
            this.functions.Add("session start", SessionStartHandler);
            this.functions.Add("session stop", SessionStopHandler);

            this.tcpClient = tcpClient;

            this.stream = this.tcpClient.GetStream();
            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, new AsyncCallback(OnLengthBytesReceived), null);
        }

        private void SessionStartHandler(JObject obj)
        {
            SendData(new JsonFile
            {
                StatusCode = (int)StatusCodes.OK,
                OppCode = OperationCodes.SESSION_START,

                Data = new JsonData
                {
                    ChatMessage = "sessie wordt nu GESTART"
                }
            });
        }

        private void SessionStopHandler(JObject obj)
        {
            SendData(new JsonFile
            {
                StatusCode = (int)StatusCodes.OK,
                OppCode = OperationCodes.SESSION_STOP,

                Data = new JsonData
                {
                    ChatMessage = "Sessie wordt nu GESTOPT"
                }
            });
        }
        
        private void OnLengthBytesReceived(IAsyncResult ar)
        {
            dataBuffer = new byte[BitConverter.ToInt32(lengthBytes)];
            stream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnDataReceived, null);
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            stream.EndRead(ar);
            JObject data = JObject.Parse(Encoding.UTF8.GetString(this.dataBuffer));
            
            handleData(data);
            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);
        }

        public void SendData(JsonFile jsonFile)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(
                jsonFile,
                Formatting.Indented,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));

            stream.Write(BitConverter.GetBytes(dataBytes.Length));
            stream.Write(dataBytes);
        }


        private void ChatHandler(JObject packetData)
        {
            SendData(new JsonFile
            {
                StatusCode = (int)StatusCodes.OK,
                OppCode = OperationCodes.CHAT,

                Data = new JsonData
                {
                    ChatMessage = "Dit is de response van uit de server, het bericht is: " +
                                  packetData["Data"]["ChatMessage"]
                }
            });
        }

        private void LoginFeature(JObject packetData)
        {
            string username = packetData.Value<string>("username");

            string password = packetData.Value<string>("password");

            if (username == password)
            {
                SendData(new JsonFile
                {
                    StatusCode = (int)StatusCodes.OK,
                    OppCode = OperationCodes.LOGIN,

                    Data = new JsonData
                    {
                        Content = "OK je bent goed ingelogd"
                    }
                });

                this.UserName = username;
            }
            else
            {
                SendData(new JsonFile
                {
                    StatusCode = (int)StatusCodes.NOT_FOUND,
                    OppCode = OperationCodes.LOGIN,

                    Data = new JsonData
                    {
                        Content = "ERROR: Verkeerde gebruiksnaam of Wachtwoord!"
                    }
                });
            }
        }

        private void handleData(JObject packetData)
        {
            Console.WriteLine($"Got a packet server: {packetData.Value<string>("OppCode")}");
            Action<JObject> action;


            if (this.functions.TryGetValue(packetData.Value<string>("OppCode"), out action))
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