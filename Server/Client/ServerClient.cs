using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using RemoteHealthcare.CentralServer.Models;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.Server.ServerClient
{
    internal class ServerClient
    {
        private SocketClient _client;
        private PatientData _patientData;

        public string UserName { get; set; }
        private Dictionary<string, Action<DataPacket>> _functions;

        //Set-ups the client constructor
        public ServerClient(SocketClient client)
        {
            _client = client;
            _client.OnMessage += (sender, data) =>
            {
                var dataPacket = JsonConvert.DeserializeObject<DataPacket>(data);
                
                //gives the JObject as parameter to determine which methode will be triggerd
                HandleData(dataPacket);
            };
            
            _functions = new Dictionary<string, Action<DataPacket>>();
            _functions.Add("login", LoginFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            
            _patientData = new PatientData();
        }

        //determines which methode exactly will be executed 
        private void HandleData(DataPacket packetData)
        {
            Console.WriteLine($"Got a packet server: {packetData.OpperationCode}");

            //Checks if the OppCode (OperationCode) does exist.
            if (_functions.TryGetValue(packetData.OpperationCode, out var action)) {
                action.Invoke(packetData);
            }else {
                throw new Exception("Function not implemented");
            }
        }
        
        //This methode used to send an request from the Server to the Client
        //The parameter is an JsonFile object
        private void SendData(DAbstract packet)
        {
            _client.SendAsync(packet).GetAwaiter().GetResult();
        }

        //the methode for the chat request
        private void ChatHandler(DataPacket packetData)
        {
            SendData(new DataPacket<ChatPacketResponse>
            {
                OpperationCode = OperationCodes.CHAT,

                data = new ChatPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Dit is de response van uit de server, het bericht is: " +
                              packetData.GetData<ChatPacketRequest>().message
                }
            });
        }
    
        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            Patient patient = new Patient(packetData.GetData<LoginPacketRequest>().username, packetData.GetData<LoginPacketRequest>().password, "1234");
            _patientData.Patients.Add(new Patient("user", "password123", "1234"));
            
            if (_patientData.MatchLoginData(patient)) {
                SendData(new DataPacket<LoginPacketResponse> {
                    OpperationCode = OperationCodes.LOGIN,
                
                    data = new LoginPacketResponse() {
                        statusCode = StatusCodes.OK,
                        message =  "Gefeliciteerd! : Je bent ingelogd"
                    }
                });

            } else {
                SendData(new DataPacket<ChatPacketResponse> {
                    OpperationCode = OperationCodes.LOGIN,
                
                    data = new ChatPacketResponse() {
                        statusCode = StatusCodes.NOT_FOUND,
                        message =  "Error: verkeerde wachtwoord of gebruikersnaam"
                    }
                });
            }
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStartPacketResponse> {
                OpperationCode = OperationCodes.SESSION_START,
                
                data = new SessionStartPacketResponse() {
                    statusCode = StatusCodes.OK,
                    message =  "Sessie wordt nu GESTART" 
                }
            });
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStopPacketResponse> {
                OpperationCode = OperationCodes.SESSION_STOP,
                
                data = new SessionStopPacketResponse() {
                    statusCode = StatusCodes.OK,
                    message =  "Sessie wordt nu GESTOPT" 
                }
            });
        }
    }
}