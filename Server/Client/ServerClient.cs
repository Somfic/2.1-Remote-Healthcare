using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Models;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server.Client
{
    public class ServerClient
    {
        private readonly Log _log = new(typeof(ServerClient));
        public SocketClient Client { get; private set; }
        private Patient _patient;
        
        private string _userId { get; set; }
        private bool _isDoctor;

        private readonly string _patientDataLocation = Environment.CurrentDirectory;

        
        private readonly Dictionary<string, Action<DataPacket>> _callbacks;


        //Set-ups the client constructor
        public ServerClient(SocketClient client)
        {
            Client = client;
          
            Client.OnMessage += (sender, data) =>
            {
                var dataPacket = JsonConvert.DeserializeObject<DataPacket>(data);

                //gives the JObject as parameter to determine which methode will be triggered
                HandleData(dataPacket);
            };

            Client.OnDisconnect += (sender, data) =>
            {
                _patient.SaveSessionData(_patientDataLocation);
            };
            
            //Fill the dictionary _callbacks with the right values
            _callbacks = new Dictionary<string, Action<DataPacket>>
            {
                { OperationCodes.Login, LoginFeature },
                { OperationCodes.Users, RequestConnectionsFeature },
                { OperationCodes.Chat, ChatHandler },
                { OperationCodes.SessionStart, SessionStartHandler },
                { OperationCodes.SessionStop, SessionStopHandler },
                { OperationCodes.Disconnect, DisconnectHandler },
                { OperationCodes.EmergencyStop, EmergencyStopHandler },
                { OperationCodes.GetPatientData, GetPatientDataHandler },
                { OperationCodes.Bikedata, GetBikeData },
                { OperationCodes.GetPatientSesssions, GetPatientSessionHandler },
                { OperationCodes.SetResistance, SetResistance }
            };
        }

        /// <summary>
        /// It checks if the packetData.OperationCode exists in the _callbacks dictionary, if it does, it invokes the
        /// action
        /// </summary>
        /// <param name="packetData">This is the data that is being sent from the server.</param>
        private void HandleData(DataPacket packetData)
        {
            _log.Debug($"Got a packet server: {packetData.OpperationCode}");

            //Checks if the OppCode (OperationCode) does exist.
            if (_callbacks.TryGetValue(packetData.OpperationCode, out var action)) 
            {
                action.Invoke(packetData);
            } else {
                throw new Exception("Function not implemented");
            }
        }
        
        /// <summary>
        /// It sends a packet to a target, if the message is not a chat the message gets returned to the sender.
        /// </summary>
        /// <param name="packetData">The packet you want to send.</param>
        /// <param name="targetId">The id of the target. Used packet contains chat, target is used to determine the
        ///                         target of the chat message. If null, the packet will be sent to the doctor that
        ///                         is online.</param>
        private void SendData(DAbstract packetData, string? targetId = null)
        {
            _log.Debug($"sending (single target): {packetData.ToJson()} \\nTarget: {targetId}");

            if (packetData.ToJson().Contains("chat"))
                CalculateTarget(targetId).Client.SendAsync(packetData).GetAwaiter().GetResult();
            else
                Client.SendAsync(packetData).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// It sends a packet to a list of targets
        /// </summary>
        /// <param name="packetData">The packet that is being sent.</param>
        /// <param name="targetIds">The list of targetIds to send the packet to.</param>
        private void SendData(DAbstract packetData, List<string> targetIds)
        {
            _log.Debug($"sending (multiple targets): {packetData.ToJson()}");
            if (packetData.ToJson().Contains("chat"))
            {
                foreach (string targetId in targetIds)
                {
                    CalculateTarget(targetId).Client.SendAsync(packetData).GetAwaiter().GetResult();
                }
            }
        }
        
        
        /// <summary>
        /// This function is called when the client sends a bike data packet to the server. The server then checks if the
        /// session id is already in the list of sessions. If it is, it adds the data to the session and sends it to the
        /// doctor. If it isn't, it adds the session to the list of sessions and sends the data to the doctor
        /// </summary>
        /// <param name="packetData">This is the object that is sent from the client to the server.</param>
        /// <returns>
        /// The data is being returned to the doctor.
        /// </returns>
        private void GetBikeData(DataPacket packetData)
        {
            BikeDataPacket data = packetData.GetData<BikeDataPacket>();

            foreach(SessionData session in _patient.Sessions)
            {
                if (session.SessionId.Equals(data.SessionId))
                {
                    session.addMiniData(data.SessionId,(int)data.speed, (int)data.distance, data.heartRate, data.elapsed.Seconds, data.deviceType, data.id);
                    _log.Critical(data.distance.ToString(CultureInfo.InvariantCulture));
                    
                    DataPacket<BikeDataPacketDoctor> dataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
                    {
                        OpperationCode = OperationCodes.Bikedata,
                        data = new BikeDataPacketDoctor()
                        {
                            distance = data.distance,
                            elapsed = data.elapsed,
                            heartRate = data.heartRate,
                            id = _userId,
                            speed = data.speed
                        }
                    };
                    
                    CalculateTarget().Client.SendAsync(dataPacketDoctor).GetAwaiter().GetResult();
                    return;
                }
            }
            
            _patient.Sessions.Add(new SessionData(data.SessionId, data.deviceType, data.id));
            _patient.SaveSessionData(_patientDataLocation);

            DataPacket<BikeDataPacketDoctor> firstDataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
            {
                OpperationCode = OperationCodes.Bikedata,
                data = new BikeDataPacketDoctor()
                {
                    distance = data.distance,
                    elapsed = data.elapsed,
                    heartRate = data.heartRate,
                    id = _userId,
                    speed = data.speed
                }
            };
            CalculateTarget().Client.SendAsync(firstDataPacketDoctor).GetAwaiter().GetResult();
            
            GetBikeData(packetData);
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
        private ServerClient CalculateTarget(string? userId = null)
        {
            foreach (ServerClient client in Server._connectedClients)
            {
                if (userId == null && client._isDoctor)
                    return client;

                if (userId != null && client._userId.Equals(userId))
                    return client;
            }

            _log.Error($"No client found for the id: {userId}");
            return null;
        }

        /// <summary>
        /// It removes all the doctors from the list of connected clients and sends the remaining clients to the client
        /// </summary>
        /// <param name="packetData">The data packet that was received from the client.</param>
        private void RequestConnectionsFeature(DataPacket packetData)
        {
            string clients = "";
            int clientCount = 0;
            List<ServerClient> connections = new(Server._connectedClients);

            connections.RemoveAll(client => client._isDoctor);


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

            SendData(new DataPacket<ConnectedClientsPacketResponse>
            {
                OpperationCode = OperationCodes.Users,

                data = new ConnectedClientsPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    connectedIds = clients
                }
            });
        }

        /// <summary>
        /// The function takes a packet of data from the client, and sends it to the patient
        /// </summary>
        /// <param name="packetData">This is the data that is sent from the client.</param>
        /// <returns>
        /// The resistance value is being returned to the client.
        /// </returns>
        private void SetResistance(DataPacket packetData)
        {
            SetResistancePacket data = packetData.GetData<SetResistancePacket>();

            ServerClient patient = Server._connectedClients.Find(patient => patient._userId == data.receiverId);

            if (patient == null) return;
            patient.SendData(new DataPacket<SetResistanceResponse>
            {
                OpperationCode = OperationCodes.SetResistance,

                data = new SetResistanceResponse()
                {
                   statusCode = StatusCodes.OK,
                   resistance = data.resistance
                }
            });
        }
        
        /// <summary>
        /// It sends a chat message to the specified user(s) or to all users if no user is specified
        /// </summary>
        /// <param name="packetData">The data packet that was received from the client.</param>
        private void ChatHandler(DataPacket packetData)
        {
            ChatPacketRequest data = packetData.GetData<ChatPacketRequest>();

            if (data.receiverId == null)
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Chat,

                    data = new ChatPacketResponse()
                    {
                        statusCode = StatusCodes.OK,
                        senderName = data.senderName,
                        senderId = data.senderId,
                        message = data.message
                    }
                }) ;
            }
            else
            {
                List<string> targetIds = data.receiverId.Split(";").ToList();
                

                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Chat,

                    data = new ChatPacketResponse()
                    {
                        statusCode = StatusCodes.OK,
                        senderId = data.senderId,
                        senderName = data.senderName,
                        message = data.message
                    }
                }, targetIds);
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            Patient? patient = null;
            Doctor? doctor = null;
            if (!packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                patient = new Patient(packetData.GetData<LoginPacketRequest>().userName,
                    packetData.GetData<LoginPacketRequest>().password);
                
                _log.Information($"Patient name: {patient.Username} Password: {patient.Password}");
            }
            else if (packetData.GetData<LoginPacketRequest>().isDoctor)
            {
                doctor = new Doctor(packetData.GetData<LoginPacketRequest>().userName,
                    packetData.GetData<LoginPacketRequest>().password, "Dhr145");
                Server._doctorData.Doctor = new Doctor("Piet", "dhrPiet", "Dhr145");

                _log.Information($"Doctor name: {doctor.Username} Password: {doctor.Password}");
            }
            
            if (patient != null && Server._patientData.MatchLoginData(patient))
            {
                _userId = patient.UserId;
                this._patient = patient;
                _isDoctor = false;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    data = new LoginPacketResponse()
                    {
                        userId = patient.UserId,
                        userName = patient.Username,
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
                    OpperationCode = OperationCodes.Login,

                    data = new LoginPacketResponse()
                    {
                        userId = doctor.UserId,
                        userName = doctor.Username,
                        statusCode = StatusCodes.OK,
                        message = "U bent succesvol ingelogd."
                    }
                });
            }
            else
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    data = new ChatPacketResponse()
                    {
                        statusCode = StatusCodes.NOT_FOUND,
                        message = "Opgegeven wachtwoord of gebruikersnaam incorrect."
                    }
                });
            }
        }
        
        //The methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
            SessionStartPacketRequest data = obj.GetData<SessionStartPacketRequest>();

            ServerClient patient = Server._connectedClients.Find(patient => patient._userId == data.selectedPatient);

            StatusCodes StatusCodes;
           
            
            //Checks if the Patient exist or not, on the result of that will be de _statusCode filled with a value.
            if (patient == null) {
                StatusCodes = StatusCodes.NOT_FOUND;   
            } else {
                StatusCodes = StatusCodes.OK;
                
                //Sends request to the Patient
                patient.SendData(new DataPacket<SessionStartPacketResponse>
                {
                    OpperationCode = OperationCodes.SessionStart,
                    data = new SessionStartPacketResponse()
                    {
                        statusCode = StatusCodes,
                        message = "Sessie wordt nu gestart."
                    }
                });
            }
            
            //Sends request to the Doctor
            SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStart,

                data = new SessionStartPacketResponse()
                {
                    statusCode = StatusCodes,
                    message = "Sessie wordt nu gestart."
                }
            });
        }

        //The methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
            SessionStopPacketRequest data = obj.GetData<SessionStopPacketRequest>();

            //Trys to Find the Patient in the _connectedClients.
            ServerClient _selectedPatient = Server._connectedClients.Find(c => c._userId == data.selectedPatient);

            if (_selectedPatient == null) return;
            
            _selectedPatient.SendData(new DataPacket<SessionStopPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStop,

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
            CalculateTarget(obj.GetData<EmergencyStopPacket>().clientId).SendData(obj);
        }

        //The methode when the Doctor disconnects a Patient.
        private void DisconnectHandler(DataPacket obj)
        {
            Server.Disconnect(this);
            Client.DisconnectAsync();

            SendData(new DataPacket<DisconnectPacketResponse>
            {
                OpperationCode = OperationCodes.Disconnect,

                data = new DisconnectPacketResponse()
                {
                    statusCode = StatusCodes.OK,
                    message = "Gebruiker wordt nu gedisconnect!"
                }
            });
        }

        public override string ToString()
        {
            return $"_userId: {_userId}, Is Doctor: {_isDoctor}, " +
                   $"IP Address: {((IPEndPoint)Client.Socket.Client.RemoteEndPoint).Address}, " +
                   $"Port: {((IPEndPoint)Client.Socket.Client.RemoteEndPoint).Port}";
        }

        /// <summary>
        /// This function is called when the client sends a request to the server to get all the _patient data. The server
        /// then sends back all the _patient data to the client
        /// </summary>
        /// <param name="packetData">This is the data packet that is sent from the client to the server.</param>
        private void GetPatientDataHandler(DataPacket packetData)
        {
            JObject[] jObjects = Server._patientData.GetPatientDataAsJObjects();
            SendData(new DataPacket<GetAllPatientsDataResponse>
            {
                OpperationCode = OperationCodes.GetPatientData,

                data = new GetAllPatientsDataResponse()
                {
                    statusCode = StatusCodes.OK,
                    JObjects = jObjects,
                    message = "Got _patient data from server successfully"
                }
            });
        }

        /// <summary>
        /// This function is called when the doctor client requests all the patient data from the server
        /// </summary>
        /// <param name="packetData">This is the data that is sent from the client to the server.</param>
        private void GetPatientSessionHandler(DataPacket packetData)
        {
            JObject[] jObjects =
                Server._patientData.GetPatientSessionsAsJObjects(packetData
                    .GetData<AllSessionsFromPatientRequest>().userId, _patientDataLocation);
            SendData(new DataPacket<AllSessionsFromPatientResponce>
            {
                OpperationCode = OperationCodes.GetPatientSesssions,

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