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
        
        private byte[] dataBuffer;
        private readonly byte[] lengthBytes = new byte[4];

        public string UserName { get; set; }
        private Dictionary<string, Action<DataPacket>> functions;

        //Set-ups the client constructor
        public Client(TcpClient tcpClient)
        {
            this.functions = new Dictionary<string, Action<DataPacket>>();
            
            this.functions.Add("login", LoginFeature);
            this.functions.Add("chat", ChatHandler);
            this.functions.Add("session start", SessionStartHandler);
            this.functions.Add("session stop", SessionStopHandler);

            this.tcpClient = tcpClient;

            this.stream = this.tcpClient.GetStream();
            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, new AsyncCallback(OnLengthBytesReceived), null);
        }
        
        private void OnLengthBytesReceived(IAsyncResult ar)
        {
            dataBuffer = new byte[BitConverter.ToInt32(lengthBytes)];
            stream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnDataReceived, null);
        }

        //receive the request from the client and triggers the right connected methode from the request
        private void OnDataReceived(IAsyncResult ar)
        {
            stream.EndRead(ar);
            
            //converts the databuffer to JObject
            string data = Encoding.UTF8.GetString(dataBuffer);
            DataPacket dataPacket = JsonConvert.DeserializeObject<DataPacket>(data);

            //gives the JObject as parameter to determine which methode will be triggerd
            handleData(dataPacket);
            stream.BeginRead(lengthBytes, 0, lengthBytes.Length, OnLengthBytesReceived, null);
        }
        
        //determines which methode exactly will be executed 
        private void handleData(DataPacket packetData)
        {
            Console.WriteLine($"Got a packet server: {packetData.OpperationCode}");
            Action<DataPacket> action;

            //Checks if the OppCode (OperationCode) does exist.
            if (functions.TryGetValue(packetData.OpperationCode, out action)) {
                action.Invoke(packetData);
            }else {
                throw new Exception("Function not implemented");
            }
        }
        //This methode used to send an request from the Server to the Client
        //The parameter is an JsonFile object
        public void SendData(DAbstract packet)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(packet.ToJson());

            stream.Write(BitConverter.GetBytes(dataBytes.Length));
            stream.Write(dataBytes);
        }

        //the methode for the chat request
        private void ChatHandler(DataPacket packetData)
        {
            SendData(new DataPacket<ChatResponse> {
                OpperationCode = OperationCodes.LOGIN,
                
                data = new ChatResponse() {
                    statusCode = StatusCodes.OK,
                    message =  "Dit is de response van uit de server, het bericht is: " +
                               packetData.GetData<ChatPacket>().message
                }
            });
        }
    
        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            string username = packetData.GetData<LoginPacket>().username;

            string password = packetData.GetData<LoginPacket>().password;

            if (username == password)
            {
                SendData(new DataPacket<LoginResponsePacket> {
                    OpperationCode = OperationCodes.LOGIN,
                
                    data = new LoginResponsePacket() {
                        statusCode = StatusCodes.OK,
                        message =  "Gefeliciteer: Je bent ingelogd"
                    }
                });

                this.UserName = username;
            } else {
                SendData(new DataPacket<ChatResponse> {
                    OpperationCode = OperationCodes.LOGIN,
                
                    data = new ChatResponse() {
                        statusCode = StatusCodes.NOT_FOUND,
                        message =  "Error: verkeerde wachtwoord of gebruikersnaam"
                    }
                });
            }
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStartResponse> {
                OpperationCode = OperationCodes.SESSION_START,
                
                data = new SessionStartResponse() {
                    statusCode = StatusCodes.OK,
                    message =  "Sessie wordt nu GESTART" 
                }
            });
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStopResponse> {
                OpperationCode = OperationCodes.SESSION_STOP,
                
                data = new SessionStopResponse() {
                    statusCode = StatusCodes.OK,
                    message =  "Sessie wordt nu GESTOPT" 
                }
            });
        }
    }
}