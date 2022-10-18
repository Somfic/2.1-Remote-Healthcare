using System.Net;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server.Client
{
    public class ServerClient
    {
        private readonly Log _log = new(typeof(ServerClient));
        
        public SocketClient Client { get; private set; }
        private PatientData _patientData;
        private DoctorData _doctorData;
        private string _userId;
        private bool _isDoctor;


        public string UserName { get; set; }
        private Dictionary<string, Action<DataPacket>> _functions;

        //Set-ups the client constructor
        public ServerClient(SocketClient client)
        {
            Client = client;
            Client.OnMessage += (sender, data) =>
            {
                var dataPacket = JsonConvert.DeserializeObject<DataPacket>(data);

                //gives the JObject as parameter to determine which methode will be triggerd
                HandleData(dataPacket);
            };

            _functions = new Dictionary<string, Action<DataPacket>>();
            _functions.Add("login", LoginFeature);
            _functions.Add("users", RequestConnectionsFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("disconnect", DisconnectHandler);
            _functions.Add("emergency stop", EmergencyStopHandler);
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
        private void SendData(DAbstract packet, string? targetId = null)
        {
            _log.Debug($"sending: {packet.ToJson()}");
            if (packet.ToJson().Contains("chat"))
                CalculateTarget(targetId).Client.SendAsync(packet).GetAwaiter().GetResult();
            else
                Client.SendAsync(packet).GetAwaiter().GetResult();
        }

        //This methode used to send an request from the Server to the Client
        //The parameter is an JsonFile object
        private void SendData(DAbstract packet, List<string> targetIds)
        {
            _log.Debug($"sending: {packet.ToJson()}");
            if (packet.ToJson().Contains("chat"))
            {
                foreach (string targetId in targetIds)
                    CalculateTarget(targetId).Client.SendAsync(packet).GetAwaiter().GetResult();
            }
        }

        //If userid == null, then search for doctor otherwise search for patient
        private ServerClient CalculateTarget(string? userId = null)
        {
            foreach (ServerClient client in Server.ConnectedClients)
            {
                if (userId == null && client._isDoctor)
                {
                    return client;
                }

                if (userId != null && client._userId.Equals(userId))
                {
                    _log.Warning($"Client: {client._userId}; Is Doctor: {Server.DoctorData}");
                    return client;
                }
            }

            return null;
        }

        private void RequestConnectionsFeature(DataPacket obj)
        {
            List<ServerClient> connections = new();

            foreach (ServerClient sc in Server.ConnectedClients)
            {
                if (!sc._isDoctor)
                    connections.Add(sc);
            }

            string clients = "";
            _log.Debug(
                $"RequestConnectionsFeature.clients: {clients}, Server._connectedClients.Count: {Server.ConnectedClients.Count}");

            int clientCount = 0;
            foreach (ServerClient client in connections)
            {
                if (!client._isDoctor && client._userId != null)
                {
                    //connections.count - 2 because we subtract the doctor and count is 1 up on the index.
                    if ((connections.Count - 1) <= clientCount)
                    {
                        clients += client._userId;
                    }
                    else
                    {
                        clients += client._userId + ";";
                    }

                    clientCount++;
                }
            }

            _log.Debug($"RequestConnectionsFeature.clients: {clients}");

            SendData(new DataPacket<ConnectedClientsPacketResponse>
            {
                OpperationCode = OperationCodes.Users,

                Data = new ConnectedClientsPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    ConnectedIds = clients
                }
            });
        }

        //the methode for the chat request
        private void ChatHandler(DataPacket packetData)
        {
            ChatPacketRequest data = packetData.GetData<ChatPacketRequest>();
            _log.Debug($"ChatHandler: {data.ToJson()}");

            if (data.ReceiverId == null)
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Chat,

                    Data = new ChatPacketResponse()
                    {
                        SenderId = data.SenderId,
                        StatusCode = StatusCodes.Ok,
                        Message = data.Message
                    }
                });
            }
            else
            {
                List<string>? targetIds = data.ReceiverId.Split(";").ToList();
                targetIds.ForEach(t => _log.Debug($"Target: {t}"));

                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Chat,

                    Data = new ChatPacketResponse()
                    {
                        SenderId = data.SenderId,
                        StatusCode = StatusCodes.Ok,
                        Message = data.Message
                    }
                }, targetIds);
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData) //TODO: spam on incorrect login
        {
            Patient? patient = null;
            Doctor? doctor = null;
            if (!packetData.GetData<LoginPacketRequest>().IsDoctor)
            {
                patient = new Patient(packetData.GetData<LoginPacketRequest>().Username,
                    packetData.GetData<LoginPacketRequest>().Password);
                    
                _log.Debug($"Patient name: {patient.UserId} Password: {patient.Password}");
            }
            else if (packetData.GetData<LoginPacketRequest>().IsDoctor)
            {
                doctor = new Doctor(packetData.GetData<LoginPacketRequest>().Username,
                    packetData.GetData<LoginPacketRequest>().Password, "Dhr145");
                Server.DoctorData.Doctor = new Doctor("Piet", "dhrPiet", "Dhr145");
                
                _log.Debug($"Doctor name: {doctor.Username} Password: {doctor.Password}");
            }


            if (patient != null && Server.PatientData.MatchLoginData(patient))
            {
                _userId = patient.UserId;
                _isDoctor = false;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    Data = new LoginPacketResponse()
                    {
                        UserId = patient.UserId,
                        StatusCode = StatusCodes.Ok,
                        Message = "U bent succesvol ingelogd."
                    }
                });
            }
            else if (doctor != null && Server.DoctorData.MatchLoginData(doctor))
            {
                _userId = doctor.UserId;
                _isDoctor = true;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    Data = new LoginPacketResponse()
                    {
                        UserId = doctor.UserId,
                        StatusCode = StatusCodes.Ok,
                        Message = "U bent succesvol ingelogd."
                    }
                });
            }
            else
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    Data = new ChatPacketResponse()
                    {
                        StatusCode = StatusCodes.NotFound,
                        Message = "Opgegeven wachtwoord of gebruikersnaam incorrect."
                    }
                });
            }
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {

            Console.WriteLine("Alle verbonden users zijn: "); 
            
            
            SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStart,

                Data = new SessionStartPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    Message = "Sessie wordt nu gestart."
                }
            });
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            SendData(new DataPacket<SessionStopPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStop,

                Data = new SessionStopPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    Message = "Sessie wordt nu gestopt."
                }
            });
        }

        //the methode for the emergency stop request
        private void EmergencyStopHandler(DataPacket obj)
        {
            _log.Debug("123 server client");
            SendData(new DataPacket<EmergencyStopPacketResponse>
            {
                OpperationCode = OperationCodes.EmergencyStop,

                Data = new EmergencyStopPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    Message = "Sessie wordt nu gestopt doormiddel van een noodstop"
                }
            });
        }

        private void DisconnectHandler(DataPacket obj)
        {
            Console.WriteLine("in de server-client methode disconnectHandler");
            Server.Disconnect(this);
            Client.DisconnectAsync();

            SendData(new DataPacket<DisconnectPacketResponse>
            {
                OpperationCode = OperationCodes.Disconnect,

                Data = new DisconnectPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    Message = "Gebruiker wordt nu gedisconnect!"
                }
            });
        }

        public override string ToString()
        {
            return $"UserId: {_userId}, Is Doctor: {_isDoctor}, " +
                   $"IP Adress: {((IPEndPoint)Client.EndPoint).Address}, " +
                   $"Port: {((IPEndPoint)Client.EndPoint).Port}";
        }
    }
}