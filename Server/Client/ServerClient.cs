using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server.Client
{
    internal class ServerClient
    {
        private readonly Log _log = new(typeof(ServerClient));

        private SocketClient _client;
        private PatientData _patientData;
        private DoctorData _doctorData;


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
            _functions.Add("disconnect", DisconnectHandler);
            
            _patientData = new PatientData();
            _doctorData = new DoctorData();
        }

        //determines which methode exactly will be executed 
        private void HandleData(DataPacket packetData)
        {
            _log.Debug($"Got a packet server: {packetData.OpperationCode}");

            //Checks if the OppCode (OperationCode) does exist.
            if (_functions.TryGetValue(packetData.OpperationCode, out var action))
            {
                action.Invoke(packetData);
            }
            else
            {
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
            Patient? patient = null;
            Doctor? doctor = null;
            if (!packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                patient = new Patient(packetData.GetData<LoginPacketRequest>().username,
                    packetData.GetData<LoginPacketRequest>().password, "1234");
                _patientData.Patients.Add(new Patient("user", "password123", "1234"));
                _log.Information($"Patient name: {patient.Username} Password: {patient.Password}");
            }
            else if (packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                doctor = new Doctor(packetData.GetData<LoginPacketRequest>().username,
                    packetData.GetData<LoginPacketRequest>().password, "Dhr145");
                _log.Information($"Doctor name: {doctor.username} Password: {doctor.password}");
            }


            if (_patientData.MatchLoginData(patient) && patient != null ||
                _doctorData.MatchLoginData(doctor) && doctor != null)
            {
                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.LOGIN,

                    data = new LoginPacketResponse()
                    {
                        statusCode = StatusCodes.OK,
                        message = "Gefeliciteerd! : Je bent ingelogd"
                    }
                });

            } else {
                SendData(new DataPacket<ChatPacketResponse> {
                    OpperationCode = OperationCodes.LOGIN,

                    data = new ChatPacketResponse()
                    {
                        statusCode = StatusCodes.NOT_FOUND,
                        message = "Opgegeven wachtwoord of gebruikersnaam incorrect."
                    }
                });
            }
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SESSION_START,

                data = new SessionStartPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Sessie wordt nu GESTART"
                }
            });
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStopPacketResponse>
            {
                OpperationCode = OperationCodes.SESSION_STOP,

                data = new SessionStopPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Sessie wordt nu GESTOPT"
                }
            });
        }
        
        private void DisconnectHandler(DataPacket obj)
        {
            //Console.WriteLine(_patientData.);
            Console.WriteLine("in de server-client methode disconnectHandler");
            Server.Disconnect(this);

            /*SendData(new DataPacket<DisconnectPacketResponse> {
                OpperationCode = OperationCodes.DISCONNECT,
                
                data = new DisconnectPacketResponse() {
                    statusCode = StatusCodes.OK,
                    message =  "Gebruiker wordt nu gedisconnect!" 
                }
            });*/
        }
    }
}