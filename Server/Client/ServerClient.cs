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
        public SocketClient Client { get; private set; }
        public string UserId { get; set; }
        private bool _isDoctor;
        private Patient _patient;
        private string _patientDataLocation = Environment.CurrentDirectory;
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

            Client.OnDisconnect += (sender, data) => { _patient.SaveSessionData(_patientDataLocation); };
            _functions = new Dictionary<string, Action<DataPacket>>();
            _functions.Add("login", LoginFeature);
            _functions.Add("users", RequestConnectionsFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("disconnect", DisconnectHandler);
            _functions.Add("emergency stop", EmergencyStopHandler);
            _functions.Add("get patient data", GetPatientDataHandler);
            _functions.Add("get patient sessions", GetPatientSessionHandler);
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
            } else {
                throw new Exception("Function not implemented");
            }
        }

        //This methode used to send an request from the Server to the Client
        //The parameter is an JsonFile object
        private void SendData(DAbstract packet, string? targetId = null)
        {
            _log.Critical($"sending (single target): {packet.ToJson()} \\nTarget: {targetId}");

            if (packet.ToJson().Contains("chat"))
                calculateTarget(targetId).Client.SendAsync(packet).GetAwaiter().GetResult();
            else
                Client.SendAsync(packet).GetAwaiter().GetResult();
        }

        //This methode used to send an request from the Server to the Client
        //The parameter is an JsonFile object
        private void SendData(DAbstract packet, List<string> targetIds)
        {
            _log.Critical($"sending (multiple targets): {packet.ToJson()}");
            if (packet.ToJson().Contains("chat"))
            {
                foreach (string targetId in targetIds)
                    calculateTarget(targetId).Client.SendAsync(packet).GetAwaiter().GetResult();
            }
        }
        
        private void GetBikeData(DataPacket obj)
        {
            BikeDataPacket data = obj.GetData<BikeDataPacket>();
            foreach (SessionData session in _patient.Sessions)
            {
                if (session.SessionId.Equals(data.SessionId))
                {
                    session.addData(data.SessionId,(int)data.speed, (int)data.distance, data.heartRate, data.elapsed.Seconds, data.deviceType, data.id);
                    _log.Critical(data.distance.ToString(CultureInfo.InvariantCulture));
                    
                    DataPacket<BikeDataPacketDoctor> dataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
                    {
                        OpperationCode = OperationCodes.BIKEDATA,
                        data = new BikeDataPacketDoctor()
                        {
                            distance = data.distance,
                            elapsed = data.elapsed,
                            heartRate = data.heartRate,
                            id = UserId,
                            speed = data.speed
                        }
                    };
                    
                    calculateTarget().Client.SendAsync(dataPacketDoctor).GetAwaiter().GetResult();
                    return;
                }
            }
            _patient.Sessions.Add(new SessionData(data.SessionId, data.deviceType, data.id));
            _patient.SaveSessionData(_patientDataLocation);
            _log.Critical(data.distance.ToString(CultureInfo.InvariantCulture));

            DataPacket<BikeDataPacketDoctor> firstDataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
            {
                OpperationCode = OperationCodes.BIKEDATA,
                data = new BikeDataPacketDoctor()
                {
                    distance = data.distance,
                    elapsed = data.elapsed,
                    heartRate = data.heartRate,
                    id = UserId,
                    speed = data.speed
                }
            };
            calculateTarget().Client.SendAsync(firstDataPacketDoctor).GetAwaiter().GetResult();
            
            GetBikeData(obj);
        }

        /// <summary>
        /// It loops through all the connected clients and returns the first one that matches the userId
        ///If userid == null, then search for doctor otherwise search for patient
        /// </summary>
        /// <param name="userId">The userId of the client you want to send the message to. If you want to send the message
        /// to the doctor, leave this parameter null.</param>
        /// <returns>
        /// A ServerClient object.
        /// </returns>
        private ServerClient calculateTarget(string? userId = null)
        {
            foreach (ServerClient client in Server._connectedClients)
            {
                if (userId == null && client._isDoctor)
                    return client;

                if (userId != null && client.UserId.Equals(userId))
                    return client;
            }

            _log.Error($"No client found for the id: {userId}");
            return null;
        }

        private void RequestConnectionsFeature(DataPacket obj)
        {
            List<ServerClient> connections = new(Server._connectedClients);
            
            _log.Debug(
                $"[Before]RequestConnectionsFeature.clients.Count: {connections.Count}, Server._connectedClients.Count: {Server._connectedClients.Count}");
                
            connections.RemoveAll(client => client._isDoctor);
            
            _log.Debug(
                $"[After]RequestConnectionsFeature.clients.Count: {connections.Count}, Server._connectedClients.Count: {Server._connectedClients.Count}");
                
            // foreach (ServerClient sc in Server._connectedClients)
            // {
            //     if (!sc._isDoctor)
            //         connections.Add(sc);
            // }

            string clients = "";

            int clientCount = 0;
            foreach (ServerClient client in connections)
            {
                if (!client._isDoctor && client.UserId != null)
                {
                    //connections.count - 2 because we subtract the doctor and count is 1 up on the index.
                    if ((connections.Count - 1) <= clientCount)
                    {
                        clients += client.UserId;
                    }
                    else
                    {
                        clients += client.UserId + ";";
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

            /*//LOGIN:
            SendData(new DataPacket<LoginPacketResponse>
            {
                OpperationCode = OperationCodes.LOGIN,

                data = new LoginPacketResponse()
                {
                    userId = doctor.UserId,
                    statusCode = StatusCodes.OK,
                    message = "U bent succesvol ingelogd."
                }
            });*/
        }

        //the methode for the chat request
        private void SetResiatance(DataPacket packetData)
        {
            SetResistancePacket data = packetData.GetData<SetResistancePacket>();

            ServerClient patient = Server._connectedClients.Find(patient => patient.UserId == data.receiverId);

            Console.WriteLine("selected is: " + patient.UserId);
            if (patient == null) return;
            patient.SendData(new DataPacket<SetResistanceResponse>
            {
                OpperationCode = OperationCodes.SET_RESISTANCE,

                data = new SetResistanceResponse()
                {
                   statusCode = StatusCodes.OK,
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
        private void LoginFeature(DataPacket packetData)
        {
            Patient? patient = null;
            Doctor? doctor = null;
            if (!packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                patient = new Patient(packetData.GetData<LoginPacketRequest>().username,
                    packetData.GetData<LoginPacketRequest>().password, "testUserName");

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
                UserId = patient.UserId;
                _isDoctor = false;
                
                Console.WriteLine("GUID GUID"+UserId);
                _patient = new Patient(patient.UserId, patient.UserId);

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
                UserId = doctor.UserId;
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
        
        //The methode for the session start request
        public void SessionStartHandler(DataPacket obj)
        {
            //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
            SessionStartPacketRequest data = obj.GetData<SessionStartPacketRequest>();

            //retieves the selected Patient from the GUI
            ServerClient patient = Server._connectedClients.Find(patient => patient.UserId == data.selectedPatient);
            
            StatusCodes _statusCode;
           
            
            //Checks if the Patient exist or not, on the result of that will be de _statusCode filled with a value.
            if (patient == null) {
                _statusCode = StatusCodes.NOT_FOUND;   
            } else {
                _statusCode = StatusCodes.OK;
                
                //Sends request to the Patient
                patient.SendData(new DataPacket<SessionStartPacketResponse>
                {
                    OpperationCode = OperationCodes.SESSION_START,

                    data = new SessionStartPacketResponse()
                    {
                        statusCode = _statusCode,
                        message = "Sessie wordt nu gestart."
                    }
                });
            }
            
            //Sends request to the Doctor
            SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SESSION_START,

                data = new SessionStartPacketResponse()
                {
                    statusCode = _statusCode,
                    message = "Sessie wordt nu gestart."
                }
            });
        }

        //The methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
            SessionStopPacketRequest data = obj.GetData<SessionStopPacketRequest>();

            //Trys to Find the Patient in the _connectedCLients.
            ServerClient _selectedPatient = Server._connectedClients.Find(c => c.UserId == data.selectedPatient);

            if (_selectedPatient == null) return;
            
            _selectedPatient.SendData(new DataPacket<SessionStopPacketResponse>
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
            calculateTarget(obj.GetData<EmergencyStopPacket>().clientId).SendData(obj);
        }

        //The methode when the Doctor disconnects a Patient.
        private void DisconnectHandler(DataPacket obj)
        {
            _log.Debug("ServerClient: disconnectHandler");
            Server.Disconnect(this);
            //Disconnects TCP-side
            Client.DisconnectAsync();

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
            return $"UserId: {UserId}, Is Doctor: {_isDoctor}, " +
                   $"IP Adress: {((IPEndPoint)Client.Socket.Client.RemoteEndPoint).Address}, " +
                   $"Port: {((IPEndPoint)Client.Socket.Client.RemoteEndPoint).Port}";
        }

        /// <summary>
        /// This function is called when the client sends a request to the server to get all the _patient data. The server
        /// then sends back all the _patient data to the client
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
                    message = "Got _patient data from server successfully"
                }
            });
        }

        private void GetPatientSessionHandler(DataPacket packetData)
        {
            _log.Debug($"Got request all patientdata from doctor client: {packetData.OpperationCode}");

            JObject[] jObjects =
                Server._patientData.GetPatientSessionsAsJObjects(packetData
                    .GetData<AllSessionsFromPatientRequest>().userId, _patientDataLocation);
            SendData(new DataPacket<AllSessionsFromPatientResponce>
            {
                OpperationCode = OperationCodes.GET_PATIENT_SESSSIONS,

                data = new AllSessionsFromPatientResponce()
                {
                    statusCode = StatusCodes.OK,
                    JObjects = jObjects,
                    message = "Got patient data from server successfully"
                }
            });
        }
    }
}