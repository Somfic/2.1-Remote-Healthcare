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
        private Patient _patient;
        
        private string UserId { get; set; }
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
            _log.Debug($"Got a packet: \r\n{packetData.ToJson()}");

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
                foreach (var targetId in targetIds)
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
            var data = packetData.GetData<BikeDataPacket>();

            foreach(var session in _patient.Sessions)
            {
                if (session.SessionId.Equals(data.SessionId))
                {
                    session.AddData(data.SessionId,(int)data.Speed, (int)data.Distance, data.HeartRate, data.Elapsed.Seconds, data.DeviceType, data.Id);

                    var dataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
                    {
                        OpperationCode = OperationCodes.Bikedata,
                        data = new BikeDataPacketDoctor()
                        {
                            Distance = data.Distance,
                            Elapsed = data.Elapsed,
                            HeartRate = data.HeartRate,
                            Id = UserId,
                            Speed = data.Speed
                        }
                    };
                    
                    CalculateTarget().Client.SendAsync(dataPacketDoctor).GetAwaiter().GetResult();
                    return;
                }
            }
            
            _patient.Sessions.Add(new SessionData(data.SessionId, data.DeviceType, data.Id));
            _patient.SaveSessionData(_patientDataLocation);

            var firstDataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
            {
                OpperationCode = OperationCodes.Bikedata,
                data = new BikeDataPacketDoctor()
                {
                    Distance = data.Distance,
                    Elapsed = data.Elapsed,
                    HeartRate = data.HeartRate,
                    Id = UserId,
                    Speed = data.Speed
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
            foreach (var client in Server.ConnectedClients)
            {
                if (userId == null && client._isDoctor)
                    return client;

                if (userId != null && client.UserId.Equals(userId))
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
            var clients = "";
            var clientCount = 0;
            List<ServerClient> connections = new(Server.ConnectedClients);

            connections.RemoveAll(client => client._isDoctor);


            foreach (var client in connections)
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

            SendData(new DataPacket<ConnectedClientsPacketResponse>
            {
                OpperationCode = OperationCodes.Users,

                data = new ConnectedClientsPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    ConnectedIds = clients
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
            var data = packetData.GetData<SetResistancePacket>();

            var patient = Server.ConnectedClients.Find(patient => patient.UserId == data.ReceiverId);

            if (patient == null) return;
            patient.SendData(new DataPacket<SetResistanceResponse>
            {
                OpperationCode = OperationCodes.SetResistance,

                data = new SetResistanceResponse()
                {
                   StatusCode = StatusCodes.Ok,
                   Resistance = data.Resistance
                }
            });
        }
        
        /// <summary>
        /// It sends a chat message to the specified user(s) or to all users if no user is specified
        /// </summary>
        /// <param name="packetData">The data packet that was received from the client.</param>
        private void ChatHandler(DataPacket packetData)
        {
            var data = packetData.GetData<ChatPacketRequest>();

            if (data.ReceiverId == null)
            {
                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Chat,

                    data = new ChatPacketResponse()
                    {
                        StatusCode = StatusCodes.Ok,
                        SenderName = data.SenderName,
                        SenderId = data.SenderId,
                        Message = data.Message
                    }
                }) ;
            }
            else
            {
                var targetIds = data.ReceiverId.Split(";").ToList();
                

                SendData(new DataPacket<ChatPacketResponse>
                {
                    OpperationCode = OperationCodes.Chat,

                    data = new ChatPacketResponse()
                    {
                        StatusCode = StatusCodes.Ok,
                        SenderId = data.SenderId,
                        SenderName = data.SenderName,
                        Message = data.Message
                    }
                }, targetIds);
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            Patient? patient = null;
            Doctor? doctor = null;
            if (!packetData.GetData<LoginPacketRequest>().IsDoctor)
            {
                patient = new Patient(packetData.GetData<LoginPacketRequest>().UserName,
                    packetData.GetData<LoginPacketRequest>().Password);
                
                _log.Information($"Patient name: {patient.Username} Password: {patient.Password}");
            }
            else if (packetData.GetData<LoginPacketRequest>().IsDoctor)
            {
                doctor = new Doctor(packetData.GetData<LoginPacketRequest>().UserName,
                    packetData.GetData<LoginPacketRequest>().Password, "Dhr145");
                Server.DoctorData.Doctor = new Doctor("Piet", "dhrPiet", "Dhr145");

                _log.Information($"Doctor name: {doctor.Username} Password: {doctor.Password}");
            }
            
            if (patient != null && Server.PatientData.MatchLoginData(patient))
            {
                UserId = patient.UserId;
                this._patient = patient;
                _isDoctor = false;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    data = new LoginPacketResponse()
                    {
                        UserId = patient.UserId,
                        UserName = patient.Username,
                        StatusCode = StatusCodes.Ok,
                        Message = "U bent succesvol ingelogd."
                    }
                });
            }
            else if (doctor != null && Server.DoctorData.MatchLoginData(doctor))
            {
                UserId = doctor.UserId;
                _isDoctor = true;

                SendData(new DataPacket<LoginPacketResponse>
                {
                    OpperationCode = OperationCodes.Login,

                    data = new LoginPacketResponse()
                    {
                        UserId = doctor.UserId,
                        UserName = doctor.Username,
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

                    data = new ChatPacketResponse()
                    {
                        StatusCode = StatusCodes.NotFound,
                        Message = "Opgegeven wachtwoord of gebruikersnaam incorrect."
                    }
                });
            }
        }
        
        //The methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
            var data = obj.GetData<SessionStartPacketRequest>();

            var patient = Server.ConnectedClients.Find(patient => patient.UserId == data.SelectedPatient);

            StatusCodes statusCodes;
           
            
            //Checks if the Patient exist or not, on the result of that will be de _statusCode filled with a value.
            if (patient == null) {
                statusCodes = StatusCodes.NotFound;   
            } else {
                statusCodes = StatusCodes.Ok;
                
                //Sends request to the Patient
                patient.SendData(new DataPacket<SessionStartPacketResponse>
                {
                    OpperationCode = OperationCodes.SessionStart,
                    data = new SessionStartPacketResponse()
                    {
                        StatusCode = statusCodes,
                        Message = "Sessie wordt nu gestart."
                    }
                });
            }
            
            //Sends request to the Doctor
            SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStart,

                data = new SessionStartPacketResponse()
                {
                    StatusCode = statusCodes,
                    Message = "Sessie wordt nu gestart."
                }
            });
        }

        //The methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
            var data = obj.GetData<SessionStopPacketRequest>();

            //Trys to Find the Patient in the _connectedClients.
            var selectedPatient = Server.ConnectedClients.Find(c => c.UserId == data.SelectedPatient);

            if (selectedPatient == null) return;
            
            selectedPatient.SendData(new DataPacket<SessionStopPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStop,

                data = new SessionStopPacketResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    Message = "Sessie wordt nu gestopt."
                }
            });
            selectedPatient._patient.SaveSessionData(Environment.CurrentDirectory);
        }

        //the methode for the emergency stop request
        private void EmergencyStopHandler(DataPacket obj)
        {
            CalculateTarget(obj.GetData<EmergencyStopPacket>().ClientId)._patient.SaveSessionData(Environment.CurrentDirectory);
            CalculateTarget(obj.GetData<EmergencyStopPacket>().ClientId).SendData(obj);
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
                    StatusCode = StatusCodes.Ok,
                    Message = "Gebruiker wordt nu gedisconnect!"
                }
            });
        }

        public override string ToString()
        {
            return $"_userId: {UserId}, Is Doctor: {_isDoctor}, " +
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
            var jObjects = Server.PatientData.GetPatientDataAsJObjects();
            SendData(new DataPacket<GetAllPatientsDataResponse>
            {
                OpperationCode = OperationCodes.GetPatientData,

                data = new GetAllPatientsDataResponse()
                {
                    StatusCode = StatusCodes.Ok,
                    JObjects = jObjects,
                    Message = "Got _patient data from server successfully"
                }
            });
        }

        /// <summary>
        /// This function is called when the doctor client requests all the patient data from the server
        /// </summary>
        /// <param name="packetData">This is the data that is sent from the client to the server.</param>
        private void GetPatientSessionHandler(DataPacket packetData)
        {
            var jObjects =
                Server.PatientData.GetPatientSessionsAsJObjects(packetData
                    .GetData<AllSessionsFromPatientRequest>().UserId, _patientDataLocation);
            SendData(new DataPacket<AllSessionsFromPatientResponce>
            {
                OpperationCode = OperationCodes.GetPatientSesssions,

                data = new AllSessionsFromPatientResponce()
                {
                    StatusCode = StatusCodes.Ok,
                    JObjects = jObjects,
                    Message = "Got patient data from server successfully"
                }
            });
        }
    }
}