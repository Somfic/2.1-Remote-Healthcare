using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server.Client
{
    public class ServerClient
    {
        private readonly Log _log = new(typeof(ServerClient));

        private SocketClient _client;
        private string _userId;
        private bool _isDoctor;


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
            {
                calculateTarget(targetId)._client.SendAsync(packet).GetAwaiter().GetResult();
            }
            else
            {
                _log.Warning($"Client: {_userId}; Is Doctor: {_isDoctor}");
                _client.SendAsync(packet).GetAwaiter().GetResult();
            }
        }

        //This methode used to send an request from the Server to the Client
        //The parameter is an JsonFile object
        private void SendData(DAbstract packet, List<string> targetIds)
        {
            _log.Debug($"sending: {packet.ToJson()}");
            if (packet.ToJson().Contains("chat"))
            {
                foreach (string targetId in targetIds)
                    calculateTarget(targetId)._client.SendAsync(packet).GetAwaiter().GetResult();
            }
        }

        //If userid == null, then search for doctor otherwise search for patient
        private ServerClient calculateTarget(string? userId = null)
        {
            _log.Warning("calculating target:");
            foreach (ServerClient client in Server._connectedClients)
            {
                if (userId == null && client._isDoctor)
                {
                    _log.Warning($"Client: {client._userId}; Is Doctor: {client._isDoctor}");
                    return client;
                }

                if (userId != null && client._userId.Equals(userId))
                {
                    _log.Warning($"Client: {client._userId}; Is Doctor: {client._isDoctor}");
                    return client;
                }
            }

            _log.Error("No client found");
            return null;
        }

        private void RequestConnectionsFeature(DataPacket obj)
        {
            List<ServerClient> connections = new();

            foreach (ServerClient sc in Server._connectedClients)
            {
                if (!sc._isDoctor)
                    connections.Add(sc);
            }

            string clients = "";
            _log.Debug(
                $"RequestConnectionsFeature.clients: {clients}, Server._connectedClients.Count: {Server._connectedClients.Count}");

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
                OpperationCode = OperationCodes.USERS,

                data = new ConnectedClientsPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    connectedIds = clients
                }
            });
        }

        //the methode for the chat request
        private void ChatHandler(DataPacket packetData)
        {
            ChatPacketRequest data = packetData.GetData<ChatPacketRequest>();
            _log.Debug($"ChatHandler: {data.ToJson()}");

            if (data.receiverId == null)
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.CHAT,

                    data = new ChatPacketResponse()
                    {
                        senderId = data.senderId,
                        statusCode = StatusCodes.OK,
                        message = data.message
                    }
                });
            }
            else
            {
                List<string>? targetIds = data.receiverId.Split(";").ToList();
                targetIds.ForEach(t => _log.Debug($"Target: {t}"));

                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.CHAT,

                    data = new ChatPacketResponse()
                    {
                        senderId = data.senderId,
                        statusCode = StatusCodes.OK,
                        message = data.message
                    }
                }, targetIds);
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData) //TODO: spam on incorrect login
        {
            _log.Debug($"loginfeature: {packetData.ToJson()}");
            Patient? patient = null;
            Doctor? doctor = null;
            if (!packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                patient = new Patient(packetData.GetData<LoginPacketRequest>().username,
                    packetData.GetData<LoginPacketRequest>().password);

                _log.Debug($"Patient name: {patient.UserId} Password: {patient.Password}");
            }
            else if (packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                doctor = new Doctor(packetData.GetData<LoginPacketRequest>().username,
                    packetData.GetData<LoginPacketRequest>().password, "Dhr145");
                Server._doctorData._doctor = new Doctor("Piet", "dhrPiet", "Dhr145");

                _log.Debug($"Doctor name: {doctor.Username} Password: {doctor.Password}");
            }


            if (patient != null && Server._patientData.MatchLoginData(patient))
            {
                _userId = patient.UserId;
                _isDoctor = false;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.LOGIN,

                    data = new LoginPacketResponse()
                    {
                        userId = patient.UserId,
                        statusCode = StatusCodes.OK,
                        message = "U bent succesvol ingelogd."
                    }
                });
            }
            else if (doctor != null && Server._doctorData.MatchLoginData(doctor))
            {
                _userId = doctor.UserId;
                _isDoctor = true;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.LOGIN,

                    data = new LoginPacketResponse()
                    {
                        userId = doctor.UserId,
                        statusCode = StatusCodes.OK,
                        message = "U bent succesvol ingelogd."
                    }
                });
            }
            else
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
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
            _log.Debug("sessionstarthandler");


            SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SESSION_START,

                data = new SessionStartPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Sessie wordt nu gestart."
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
                    message = "Sessie wordt nu gestopt."
                }
            });
        }

        //the methode for the emergency stop request
        private void EmergencyStopHandler(DataPacket obj)
        {
            _log.Debug("emergencystophandler");
            SendData(new DataPacket<EmergencyStopPacketResponse>
            {
                OpperationCode = OperationCodes.EMERGENCY_STOP,

                data = new EmergencyStopPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Sessie wordt nu gestopt doormiddel van een noodstop"
                }
            });
        }

        private void DisconnectHandler(DataPacket obj)
        {
            _log.Debug("disconnectHandler");
            Server.Disconnect(this);
            _client.DisconnectAsync();

            Server.printUsers();

            SendData(new DataPacket<DisconnectPacketResponse>
            {
                OpperationCode = OperationCodes.DISCONNECT,

                data = new DisconnectPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Gebruiker wordt nu gedisconnect!"
                }
            });
        }

        public override string ToString()
        {
            return $"UserId: {_userId}, Is Doctor: {_isDoctor}, " +
                   $"IP Adress: {((IPEndPoint)_client.Socket.Client.RemoteEndPoint).Address}, " +
                   $"Port: {((IPEndPoint)_client.Socket.Client.RemoteEndPoint).Port}";
        }
    }
}