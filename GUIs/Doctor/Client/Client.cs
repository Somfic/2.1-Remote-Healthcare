using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor.Client
{
    public class Client
    {
        public SocketClient _client { get; set; } = new(true);
        private List<string> _connected;
        public List<Patient> _patientList;
        private Log _log = new(typeof(Client));

        public string password { get; set; }
        public string username { get; set; }
        public bool loggedIn { get; set; }
        private string _userId;

        private Dictionary<string, Action<DataPacket>> _functions = new();

        public Client()
        {
            loggedIn = false;
            _patientList = new List<Patient>();
            _functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _functions.Add("login", LoginFeature);
            _functions.Add("users", RequestConnectionsFeature);
            _functions.Add("chat", ChatHandler);
            _functions.Add("session start", SessionStartHandler);
            _functions.Add("session stop", SessionStopHandler);
            _functions.Add("emergency stop", EmergencyStopHandler);
            _functions.Add("get patient data", GetPatientDataHandler);

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };
        }

        public async Task RunAsync()
        {
            while (true)
            {
                //if the user isn't logged in, the user cant send any command to the server
                if (loggedIn)
                {
                    _log.Information("Voer een command in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [START SESSIE] [STOP SESSIE] [NOODSTOP]");
                    string userCommand = "";

                    if (userCommand.ToLower().Equals("bericht"))
                    {
                        SendChatAsync();
                    }
                    else if (userCommand.ToLower().Equals("start") && userCommand.ToLower().Equals("sessie"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };

                        await _client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Contains(("stop")) && userCommand.ToLower().Contains("Sessie"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };

                        await _client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Equals("noodstop"))
                    {
                        var req = new DataPacket<EmergencyStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.EMERGENCY_STOP,
                        };

                        await _client.SendAsync(req);
                    }
                    else
                    {
                        _log.Warning("Het commando dat u heeft ingevoerd is incorrect.");
                    }
                }
            }
        }

        private async void SendChatAsync()
        {
            await RequestClients();
            while (_connected == null)
            {
            }
            
            _log.Information("Voer uw bericht in: ");
            String chatInput = Console.ReadLine();
            string savedConnections = "";
            _connected.ForEach(c => savedConnections+= c + "; ");
            _log.Information($"Voor welk accountnummer is dit bedoeld: [{savedConnections}]");
            String target = Console.ReadLine();

            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _userId,
                    receiverId = target,
                    message = chatInput
                }
            };
            
            await _client.SendAsync(req);
        }

        public async Task RequestClients()
        {
            var req = new DataPacket<ConnectedClientsPacketRequest>
            {
                OpperationCode = OperationCodes.USERS
            };

            await _client.SendAsync(req);
        }

        public async Task AskForLoginAsync()
        {
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    username = username,
                    password = password,
                    isDoctor = true
                }
            };

            await _client.SendAsync(loginReq);
        }

        public async Task RequestPatientDataAsync()
        {
            DataPacket<GetAllPatientsDataRequest> patientReq = new DataPacket<GetAllPatientsDataRequest>
            {
                OpperationCode = OperationCodes.GET_PATIENT_DATA
            };

            await _client.SendAsync(patientReq);
        }

        //this methode will get the right methode that will be used for the response from the server
        public void HandleData(DataPacket packet)
        {
            //Checks if the OppCode (OperationCode) does exist.
            if (_functions.TryGetValue(packet.OpperationCode, out var action))
            {
                action.Invoke(packet);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the emergency stop request
        //TODO 
        private void EmergencyStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStartPacketResponse>().message);
        }

        //the methode for the send chat request
        private void ChatHandler(DataPacket packetData)
        {
            _log.Information(packetData.GetData<ChatPacketResponse>().message);
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().statusCode).Equals(200))
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().connectedIds.Split(";").ToList();
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().statusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().userId;
                _log.Information($"Succesfully logged in to the user: {username}; {password}; {_userId}.");
                loggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().statusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().message);
            }
        }
        
        private void GetPatientDataHandler(DataPacket packetData)
        {
            _log.Debug($"Got all patientdata from server: {packetData.OpperationCode}");
            _log.Debug($"Received: {packetData.ToJson()}");

            JObject[] jObjects = packetData.GetData<GetAllPatientsDataResponse>().JObjects;

            foreach (JObject jObject in jObjects)
            {
                Patient patient = jObject.ToObject<Patient>();
                _patientList.Add(patient);
                _log.Debug(patient.ToString());
            }

            // _log.Debug(_patientList[0].ToString());
        }
    }
}