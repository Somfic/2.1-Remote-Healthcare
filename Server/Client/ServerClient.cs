using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server.Client;

public class ServerClient
{
    private readonly Log _log = new(typeof(ServerClient));

    private readonly Dictionary<string, Action<DataPacket>> _callbacks;
    private bool _isDoctor;

    private Patient _patient;

    private readonly string _patientDataLocation = Environment.CurrentDirectory;


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

        //Fill the dictionary _callbacks with the right values
        _callbacks = new Dictionary<string, Action<DataPacket>>();
        _callbacks.Add(OperationCodes.Login, LoginFeature);
        _callbacks.Add(OperationCodes.Users, RequestConnectionsFeature);
        _callbacks.Add(OperationCodes.Chat, ChatHandler);
        _callbacks.Add(OperationCodes.SessionStart, SessionStartHandler);
        _callbacks.Add(OperationCodes.SessionStop, SessionStopHandler);
        _callbacks.Add(OperationCodes.Disconnect, DisconnectHandler);
        _callbacks.Add(OperationCodes.EmergencyStop, EmergencyStopHandler);
        _callbacks.Add(OperationCodes.GetPatientData, GetPatientDataHandler);
        _callbacks.Add(OperationCodes.Bikedata, GetBikeData);
        _callbacks.Add(OperationCodes.GetPatientSesssions, GetPatientSessionHandler);
        _callbacks.Add(OperationCodes.SetResistance, SetResiatance);
    }

    public SocketClient Client { get; }
    public string UserId { get; set; }

    public string UserName { get; set; }

    //determines which methode exactly will be executed 
    private void HandleData(DataPacket packetData)
    {
        _log.Debug($"Got a packet server: {packetData.OpperationCode}");

        //Checks if the OppCode (OperationCode) does exist.
        if (_callbacks.TryGetValue(packetData.OpperationCode, out var action))
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
        _log.Critical($"sending (single target): {packet.ToJson()} \\nTarget: {targetId}");

        if (packet.ToJson().Contains("chat"))
        {
            CalculateTarget(targetId).Client.SendAsync(packet).GetAwaiter().GetResult();
        }
        else
        {
            Client.SendAsync(packet).GetAwaiter().GetResult();
        }
    }

    //This methode used to send an request from the Server to the Client
    //The parameter is an JsonFile object
    private void SendData(DAbstract packet, List<string> targetIds)
    {
        _log.Critical($"sending (multiple targets): {packet.ToJson()}");
        if (packet.ToJson().Contains("chat"))
        {
            foreach (var targetId in targetIds)
            {
                _log.Warning(targetId);
                CalculateTarget(targetId).Client.SendAsync(packet).GetAwaiter().GetResult();
            }
        }
    }

    private void GetBikeData(DataPacket obj)
    {
        var data = obj.GetData<BikeDataPacket>();

        foreach (var session in _patient.Sessions)
        {
            if (session.SessionId.Equals(data.SessionId))
            {
                session.AddData(data.SessionId, (int)data.Speed, (int)data.Distance, data.HeartRate,
                    data.Elapsed.Seconds, data.DeviceType, data.Id);
                _log.Critical(data.Distance.ToString(CultureInfo.InvariantCulture));

                var dataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
                {
                    OpperationCode = OperationCodes.Bikedata,
                    Data = new BikeDataPacketDoctor
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

        _log.Critical(data.Distance.ToString(CultureInfo.InvariantCulture));

        var firstDataPacketDoctor = new DataPacket<BikeDataPacketDoctor>
        {
            OpperationCode = OperationCodes.Bikedata,
            Data = new BikeDataPacketDoctor
            {
                Distance = data.Distance,
                Elapsed = data.Elapsed,
                HeartRate = data.HeartRate,
                Id = UserId,
                Speed = data.Speed
            }
        };
        CalculateTarget().Client.SendAsync(firstDataPacketDoctor).GetAwaiter().GetResult();

        GetBikeData(obj);
    }

    /// <summary>
    ///     It loops through all the connected clients and returns the first one that matches the userId
    ///     If userid == null, then search for doctor otherwise search for patient
    /// </summary>
    /// <param name="userId">
    ///     The userId of the client you want to send the message to. If you want to send the message
    ///     to the doctor, leave this parameter null.
    /// </param>
    /// <returns>
    ///     A ServerClient object.
    /// </returns>
    private ServerClient CalculateTarget(string? userId = null)
    {
        foreach (var client in Server.ConnectedClients)
        {
            if (userId == null && client._isDoctor)
            {
                return client;
            }

            if (userId != null && client.UserId.Equals(userId))
            {
                return client;
            }
        }

        _log.Error($"No client found for the id: {userId}");
        return null;
    }

    private void RequestConnectionsFeature(DataPacket obj)
    {
        List<ServerClient> connections = new(Server.ConnectedClients);

        _log.Debug(
            $"[Before]RequestConnectionsFeature.clients.Count: {connections.Count}, Server._connectedClients.Count: {Server.ConnectedClients.Count}");

        connections.RemoveAll(client => client._isDoctor);

        _log.Debug(
            $"[After]RequestConnectionsFeature.clients.Count: {connections.Count}, Server._connectedClients.Count: {Server.ConnectedClients.Count}");

        // foreach (ServerClient sc in Server._connectedClients)
        // {
        //     if (!sc._isDoctor)
        //         connections.Add(sc);
        // }

        var clients = "";

        var clientCount = 0;
        foreach (var client in connections)
        {
            if (!client._isDoctor && client.UserId != null)
            {
                //connections.count - 2 because we subtract the doctor and count is 1 up on the index.
                if (connections.Count - 1 <= clientCount)
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
            OpperationCode = OperationCodes.Users,

            Data = new ConnectedClientsPacketResponse
            {
                StatusCode = StatusCodes.Ok,
                ConnectedIds = clients
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
        var data = packetData.GetData<SetResistancePacket>();

        var patient = Server.ConnectedClients.Find(patient => patient.UserId == data.ReceiverId);

        _log.Debug("selected is: " + patient.UserId);
        if (patient == null)
        {
            return;
        }

        patient.SendData(new DataPacket<SetResistanceResponse>
        {
            OpperationCode = OperationCodes.SetResistance,

            Data = new SetResistanceResponse
            {
                StatusCode = StatusCodes.Ok,
                Resistance = data.Resistance
            }
        });
    }

    private void ChatHandler(DataPacket packetData)
    {
        var data = packetData.GetData<ChatPacketRequest>();
        _log.Debug($"ChatHandler: {data.ToJson()}");

        if (data.ReceiverId == null)
        {
            SendData(new DataPacket<ChatPacketResponse>
            {
                OpperationCode = OperationCodes.Chat,

                Data = new ChatPacketResponse
                {
                    StatusCode = StatusCodes.Ok,
                    SenderName = data.SenderName,
                    SenderId = data.SenderId,
                    Message = data.Message
                }
            });
        }
        else
        {
            var targetIds = data.ReceiverId.Split(";").ToList();

            _log.Warning($"chatHandler: {data.ReceiverId}");

            targetIds.ForEach(t => _log.Debug($"Target: {t}"));

            SendData(new DataPacket<ChatPacketResponse>
            {
                OpperationCode = OperationCodes.Chat,

                Data = new ChatPacketResponse
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

            _log.Debug($"Patient name: {patient.UserId} Password: {patient.Password}");
        }
        else if (packetData.GetData<LoginPacketRequest>().IsDoctor)
        {
            doctor = new Doctor(packetData.GetData<LoginPacketRequest>().UserName,
                packetData.GetData<LoginPacketRequest>().Password, "Dhr145");
            Server.DoctorData.Doctor = new Doctor("Piet", "dhrPiet", "Dhr145");

            _log.Debug($"Doctor name: {doctor.Username} Password: {doctor.Password}");
        }


        if (patient != null && Server.PatientData.MatchLoginData(patient))
        {
            UserId = patient.UserId;
            _isDoctor = false;

            SendData(new DataPacket<LoginPacketResponse>
            {
                OpperationCode = OperationCodes.Login,

                Data = new LoginPacketResponse
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

                Data = new LoginPacketResponse
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

                Data = new ChatPacketResponse
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Opgegeven wachtwoord of gebruikersnaam incorrect."
                }
            });
        }
    }

    //The methode for the session start request
    public void SessionStartHandler(DataPacket obj)
    {
        //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
        var data = obj.GetData<SessionStartPacketRequest>();

        var patient = Server.ConnectedClients.Find(patient => patient.UserId == data.SelectedPatient);

        StatusCodes statusCode;


        //Checks if the Patient exist or not, on the result of that will be de _statusCode filled with a value.
        if (patient == null)
        {
            statusCode = StatusCodes.NotFound;
        }
        else
        {
            statusCode = StatusCodes.Ok;

            //Sends request to the Patient
            patient.SendData(new DataPacket<SessionStartPacketResponse>
            {
                OpperationCode = OperationCodes.SessionStart,
                Data = new SessionStartPacketResponse
                {
                    StatusCode = statusCode,
                    Message = "Sessie wordt nu gestart."
                }
            });
        }

        //Sends request to the Doctor
        SendData(new DataPacket<SessionStartPacketResponse>
        {
            OpperationCode = OperationCodes.SessionStart,

            Data = new SessionStartPacketResponse
            {
                StatusCode = statusCode,
                Message = "Sessie wordt nu gestart."
            }
        });
    }

    //The methode for the session stop request
    private void SessionStopHandler(DataPacket obj)
    {
        //Retrieves the DataPacket and covert it to a SessionStartPacketRequest. 
        var data = obj.GetData<SessionStopPacketRequest>();

        //Trys to Find the Patient in the _connectedCLients.
        var selectedPatient = Server.ConnectedClients.Find(c => c.UserId == data.SelectedPatient);

        if (selectedPatient == null)
        {
            return;
        }

        selectedPatient.SendData(new DataPacket<SessionStopPacketResponse>
        {
            OpperationCode = OperationCodes.SessionStop,

            Data = new SessionStopPacketResponse
            {
                StatusCode = StatusCodes.Ok,
                Message = "Sessie wordt nu gestopt."
            }
        });
    }

    //the methode for the emergency stop request
    private void EmergencyStopHandler(DataPacket obj)
    {
        CalculateTarget(obj.GetData<EmergencyStopPacket>().ClientId).SendData(obj);
    }

    //The methode when the Doctor disconnects a Patient.
    private void DisconnectHandler(DataPacket obj)
    {
        Server.Disconnect(this);
        Client.DisconnectAsync();

        Server.PrintUsers();

        SendData(new DataPacket<DisconnectPacketResponse>
        {
            OpperationCode = OperationCodes.Disconnect,

            Data = new DisconnectPacketResponse
            {
                StatusCode = StatusCodes.Ok,
                Message = "Gebruiker wordt nu gedisconnect!"
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
    ///     This function is called when the client sends a request to the server to get all the _patient data. The server
    ///     then sends back all the _patient data to the client
    /// </summary>
    /// <param name="DataPacket">This is the data packet that is sent from the client to the server.</param>
    private void GetPatientDataHandler(DataPacket packetData)
    {
        _log.Debug($"Got request all patientdata from doctor client: {packetData.OpperationCode}");

        var jObjects = Server.PatientData.GetPatientDataAsJObjects();
        SendData(new DataPacket<GetAllPatientsDataResponse>
        {
            OpperationCode = OperationCodes.GetPatientData,

            Data = new GetAllPatientsDataResponse
            {
                StatusCode = StatusCodes.Ok,
                JObjects = jObjects,
                Message = "Got _patient data from server successfully"
            }
        });
    }

    private void GetPatientSessionHandler(DataPacket packetData)
    {
        _log.Debug($"Got request all patientdata from doctor client: {packetData.OpperationCode}");

        var jObjects =
            Server.PatientData.GetPatientSessionsAsJObjects(packetData
                .GetData<AllSessionsFromPatientRequest>().UserId, _patientDataLocation);
        SendData(new DataPacket<AllSessionsFromPatientResponce>
        {
            OpperationCode = OperationCodes.GetPatientSesssions,

            Data = new AllSessionsFromPatientResponce
            {
                StatusCode = StatusCodes.Ok,
                JObjects = jObjects,
                Message = "Got patient data from server successfully"
            }
        });
    }
}