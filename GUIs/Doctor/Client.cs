using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;
using MvvmHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.GUIs.Doctor.ViewModels;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.GUIs.Doctor
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

        public ObservableObject currentViewModel; 

        private Dictionary<string, Action<DataPacket>> _functions = new();

        public Client()
        {
            loggedIn = false;
            _patientList = new List<Patient>();
            _functions = new Dictionary<string, Action<DataPacket>>();

            //Adds for each key an callback methode in the dictionary 
            _functions.Add(OperationCodes.LOGIN, LoginFeature);
            _functions.Add(OperationCodes.USERS, RequestConnectionsFeature);
            _functions.Add(OperationCodes.CHAT, ChatHandler);
            _functions.Add(OperationCodes.SESSION_START, SessionStartHandler);
            _functions.Add(OperationCodes.SESSION_STOP, SessionStopHandler);
            _functions.Add(OperationCodes.EMERGENCY_STOP, EmergencyStopHandler);
            _functions.Add(OperationCodes.GET_PATIENT_DATA, GetPatientDataHandler);

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
                    _log.Information("Voer een commando in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [START SESSIE] [STOP SESSIE] [NOODSTOP]");
                    string userCommand = "";

                    if (userCommand.ToLower().Equals("bericht"))
                    {
                        //SendChatAsync();
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

        public async void SendChatAsync(string target, string chatInput)
        {
            await requestClients();
            
            /*/* This is a while loop that will do nothing until connected is filled #1#
            while (_connected.Count == 0)
            {
                _log.Debug("Loading...");
            }
            _log.Information("escaped loading");
            string savedConnections = " ";
            foreach (string id in _connected)
            {
                savedConnections += id + "; ";
                _log.Debug($"{id} has been added, saved connections is now: [{savedConnections}]");
            }

            string? target = 0000 + "";

            /* This is a while loop that will keep asking for a target until the target is in the list of connected
            clients. #1#
            while (!_connected.Contains(target) && !target.Contains(";"))
            {
                _log.Information($"Voor welk accountnummer is dit bedoeld? Voor meerdere accountnummers tegelijk, " +
                                 $"gebruik een ; tussen de nummers. Kies uit de volgende beschikbare " +
                                 $"accountnummers: \t[{savedConnections}]");
                target = Console.ReadLine();

                //breaks the while-loop if all targets are correct.
                if (CheckTargets(target.Split(";").ToList(), _connected))
                    break;
            }

            _log.Information("Voer uw bericht in: ");
            String chatInput = Console.ReadLine();*/

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

        /// <summary>
        /// It checks if all the targets are in the connections list
        /// </summary>
        /// <param name="targets">A list of strings that represent the targets you want to check for.</param>
        /// <param name="connections">A list of all the connections that are currently active.</param>
        /// <returns>
        /// True if all targets are in connections list, false if not so.
        /// </returns>
        private bool CheckTargets(List<string> targets, List<string> connections)
        {
            foreach (string target in targets)
            {
                if (!connections.Contains(target))
                    return false;
            }

            return true;
        }

        private async Task requestClients()
        {
            if (_connected != null)
                _connected.Clear();
            var req = new DataPacket<ConnectedClientsPacketRequest>
            {
                OpperationCode = OperationCodes.USERS
            };

            await _client.SendAsync(req);
        }

        /// <summary>
        /// It sends a login request to the server.
        /// </summary>
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

            _log.Debug(loginReq.ToJson());
            
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
        private void EmergencyStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().message);
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            var sessie = obj.GetData<SessionStartPacketResponse>();
            Console.WriteLine(obj.ToJson());
            ((DoctorViewModel) currentViewModel).CurrentUserName = (sessie.statusCode.Equals(StatusCodes.OK)) ? ((DoctorViewModel) currentViewModel).CurrentUser.Username : "Gekozen Patient is niet online";
        }

        //the methode for the send chat request
        private void ChatHandler(DataPacket packetData)
        {
            _log.Information(
                $"Incomming message: {packetData.GetData<ChatPacketResponse>().senderId}: {packetData.GetData<ChatPacketResponse>().message}");
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            _log.Debug(packetData.ToJson());
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().statusCode).Equals(200))
            {
                // _log.Debug(_connected.Count.ToString());
                // _connected.RemoveRange(0, _connected.Count - 1);
                // _log.Debug(_connected.Count.ToString());
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().connectedIds.Split(";").ToList();
                _log.Critical(_connected.Count.ToString());
            }
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
        
        /// <summary>
        /// It gets all the patient data from the server and adds it to a list
        /// </summary>
        /// <param name="DataPacket">This is the object that is sent from the server to the client. It contains the data
        /// that is sent from the server.</param>
        private void GetPatientDataHandler(DataPacket packetData)
        {
            _log.Debug($"Got all patientdata from server: {packetData.OpperationCode}");
            _log.Debug($"Received: {packetData.ToJson()}");

            JObject[] jObjects = packetData.GetData<GetAllPatientsDataResponse>().JObjects;

            foreach (JObject jObject in jObjects)
            {
                Patient patient = jObject.ToObject<Patient>();
                _patientList.Add(patient);
            }
        }
    }
}
