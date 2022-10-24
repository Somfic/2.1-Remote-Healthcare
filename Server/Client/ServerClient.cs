using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Models;
using Xceed.Wpf.AvalonDock.Layout;

namespace RemoteHealthcare.Server.Client
{
    public class ServerClient
    {
        private readonly Log _log = new(typeof(ServerClient));

        public SocketClient _client;
        public string _userId { get; set; }
        private bool _isDoctor;

        private string _patientDataLocation = Path.Combine(Environment.CurrentDirectory, "PatientData");

        private Patient patient;
        
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

            _client.OnDisconnect += (sender, data) =>
            {
                patient.SaveSessionData(_patientDataLocation);
            };
            _functions = new Dictionary<string, Action<DataPacket>>();
            _functions.Add("login", LoginFeature);
            _functions.Add("users", RequestConnectionsFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("disconnect", DisconnectHandler);
            _functions.Add("emergency stop", EmergencyStopHandler);
            _functions.Add("get patient data", GetPatientDataHandler);
            _functions.Add("set resitance", SetResiatance);
            _functions.Add("bikedata", GetBikeData);

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
                calculateTarget(targetId)._client.SendAsync(packet).GetAwaiter().GetResult();
            else
                _client.SendAsync(packet).GetAwaiter().GetResult();
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

        private void GetBikeData(DataPacket obj)
        {
            BikeDataPacket data = obj.GetData<BikeDataPacket>();
            foreach(SessionData session in patient.Sessions)
            {
                if (session.SessionId.Equals(data.SessionId))
                {
                    session.addData(data.SessionId,(int)data.speed, (int)data.distance, data.heartRate, data.elapsed.Seconds, data.deviceType, data.id);
                    return;
                }
            }
            patient.Sessions.Add(new SessionData(data.SessionId, data.deviceType, data.id));
            _log.Critical(data.distance.ToString(CultureInfo.InvariantCulture));
            patient.SaveSessionData(_patientDataLocation);
            calculateTarget()._client.SendAsync(obj);
            GetBikeData(obj);
        }

        //If userid == null, then search for doctor otherwise search for patient

        private ServerClient calculateTarget(string? userId = null)
        {
            foreach (ServerClient client in Server._connectedClients)
            {
                if (userId == null && client._isDoctor)
                {
                    return client;
                }

                if (userId != null && client._userId.Equals(userId))
                {
                    _log.Warning($"Client: {client._userId}; Is Doctor: {Server._doctorData}");
                    return client;
                }
            }

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

        private void SetResiatance(DataPacket packetData)
        {
            SetResistancePacket data = packetData.GetData<SetResistancePacket>();

            ServerClient patient = Server._connectedClients.Find(patient => patient._userId == data.receiverId);

            Console.WriteLine("selected is: " + patient._userId);
            if (patient == null) return;

            patient.SendData(new DataPacket<SetResistancePacket>
            {
                OpperationCode = OperationCodes.SESSION_START,

                data = new SetResistancePacket()
                {
                   receiverId = data.receiverId,
                   resistance = data.resistance
                }
            });
        }

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
                        statusCode = StatusCodes.OK,
                        senderId = data.senderId,
                        message = data.message
                    }
                }) ;
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
                        statusCode = StatusCodes.OK,
                        senderId = data.senderId,
                        message = data.message
                    }
                });
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData) //TODO: spam on incorrect login
        {
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
                this.patient = patient;
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
         
            SessionStartPacketRequest data = obj.GetData<SessionStartPacketRequest>();

            ServerClient patient = Server._connectedClients.Find(patient => patient._userId == data.selectedPatient);

            Console.WriteLine("selected is: " + patient._userId);
            if (patient == null) return;

            patient.SendData(new DataPacket<SessionStartPacketResponse>
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
            
            SessionStopPacketRequest data = obj.GetData<SessionStopPacketRequest>();

            ServerClient tt = Server._connectedClients.Find(c => c._userId == data.selectedPatient);
        
            Console.WriteLine("gevonden id: " + tt._userId);
            Console.WriteLine("gevonden name: " + tt.UserName);
                
                
            tt.SendData(new DataPacket<SessionStopPacketResponse>
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
            _log.Debug("123 server client");
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
            _log.Debug("in de server-client methode disconnectHandler");
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

        /// <summary>
        /// This function is called when the client sends a request to the server to get all the patient data. The server
        /// then sends back all the patient data to the client
        /// </summary>
        /// <param name="DataPacket">This is the data packet that is sent from the client to the server.</param>
        private void GetPatientDataHandler(DataPacket packetData)
        {
            _log.Debug($"Got request all patientdata from doctor client: {packetData.OpperationCode}");

            JObject[] jObjects = Server._patientData.GetPatientDataAsJObjects();
            SendData(new DataPacket<GetAllPatientsDataResponse>
            {
                OpperationCode = OperationCodes.GET_PATIENT_DATA,
                
                data = new GetAllPatientsDataResponse()
                {
                    statusCode = StatusCodes.OK,
                    JObjects = jObjects,
                    message = "Got patient data from server successfully"
                }
            });
        }
    }
}