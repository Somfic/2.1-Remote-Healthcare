using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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
    public class Client : ObservableObject
    {
        public SocketClient _client { get; set; } = new(true);

        private List<string> _connected;

        public List<Patient> _patientList;
        public List<SessionData> Sessions;

        public DoctorViewModel DoctorViewModel;
        public PastSessionsViewModel PastSessionsViewModel;

        private Log _log = new(typeof(Client));
        public string password { get; set; }
        public string _userName { get; set; }
        public bool loggedIn { get; set; }
        private string _userId;

        public bool hasSessionResponce;

        private Dictionary<string, Action<DataPacket>> _callbacks = new();


        public Client()
        {
            loggedIn = false;
            hasSessionResponce = false;
            _patientList = new List<Patient>();
            _callbacks = new Dictionary<string, Action<DataPacket>>();
            Sessions = new List<SessionData>();

            //Adds for each key an callback methode in the dictionary 
            _callbacks.Add(OperationCodes.LOGIN, LoginFeature);
            _callbacks.Add(OperationCodes.USERS, RequestConnectionsFeature);
            _callbacks.Add(OperationCodes.CHAT, ChatHandler);
            _callbacks.Add(OperationCodes.SESSION_START, SessionStartHandler);
            _callbacks.Add(OperationCodes.SESSION_STOP, SessionStopHandler);
            _callbacks.Add(OperationCodes.EMERGENCY_STOP, EmergencyStopHandler);
            _callbacks.Add(OperationCodes.GET_PATIENT_DATA, GetPatientDataHandler);
            _callbacks.Add(OperationCodes.BIKEDATA, GetBikeData);
            _callbacks.Add(OperationCodes.GET_PATIENT_SESSSIONS, GetPatientSessionsHandler);

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
                        await requestClients();
                    }
                    else if (userCommand.ToLower().Equals("start") && userCommand.ToLower().Equals("sessie"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_START,
                        };
                        _log.Warning($"sending {req.ToJson()}");

                        await _client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Contains(("stop")) && userCommand.ToLower().Contains("Sessie"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SESSION_STOP,
                        };
                        _log.Warning($"sending {req.ToJson()}");

                        await _client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Equals("noodstop"))
                    {
                        var req = new DataPacket<EmergencyStopPacket>
                        {
                            OpperationCode = OperationCodes.EMERGENCY_STOP,
                        };
                        _log.Warning($"sending {req.ToJson()}");

                        await _client.SendAsync(req);
                    }
                    else
                    {
                        _log.Warning("Het commando dat u heeft ingevoerd is incorrect.");
                    }
                }
            }
        }

        public async Task SendChatAsync(string? target = null, string? chatInput = null)
        {
            _log.Debug("SendChatAsync(): entered");
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _userId,
                    senderName = _userName,
                    receiverId = target,
                    message = chatInput
                }
            };

            _log.Warning($"sending {req.ToJson()}");

            await _client.SendAsync(req);
        }

        public async void SetResistance(string target, int res)
        {
            var req = new DataPacket<SetResistancePacket>
            {
                OpperationCode = OperationCodes.SET_RESISTANCE,
                data = new SetResistancePacket()
                {
                    receiverId = target,
                    resistance = res
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
            _log.Debug(_connected.Count.ToString());
            if (_connected != null)
                _connected.Clear();
            _log.Debug(_connected.Count.ToString());
            var req = new DataPacket<ConnectedClientsPacketRequest>
            {
                OpperationCode = OperationCodes.USERS,
                data = new ConnectedClientsPacketRequest()
                {
                    requester = _userId
                }
            };
            _log.Warning($"sending {req.ToJson()}");

            await _client.SendAsync(req);
        }

        public async Task AskForLoginAsync()
        {
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    userName = _userName,
                    password = password,
                    isDoctor = true
                }
            };

            _log.Warning($"sending {loginReq.ToJson()}");

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
            _log.Debug($"Received: {packet.ToJson()}");
            //Checks if the OppCode (OperationCode) does exist.
            if (_callbacks.TryGetValue(packet.OpperationCode, out var action))
            {
                action.Invoke(packet);
            }
            else
            {
                throw new Exception("Function not implemented");
            }
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

            //Change the GUI with an Alert depends on the outcome of the IF-Statement
            DoctorViewModel.CurrentUserName = (sessie.statusCode.Equals(StatusCodes.OK))
                ? DoctorViewModel.CurrentUser.Username
                : "Gekozen Patient is niet online";
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().message);
        }
        
        //the methode for printing out the received message
        private void ChatHandler(DataPacket packetData)
        {
            ObservableCollection<string> chats = new();
            foreach (var chatMessage in DoctorViewModel._chatMessages)
                chats.Add(chatMessage);
            
            chats.Add($"{packetData.GetData<ChatPacketResponse>().senderName}: {packetData.GetData<ChatPacketResponse>().message}");
            DoctorViewModel.ChatMessages = chats;
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().statusCode).Equals(200))
            {
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().connectedIds.Split(";").ToList();
                _log.Warning($"RequestConnectionsFeature(): {_connected.Count.ToString()}");

                SendChatAsync();
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().statusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().userId;
                _userName = packetData.GetData<LoginPacketResponse>().userName;
                _log.Information($"Succesfully logged in to the user: {_userName}; {password}; {_userId}.");
                loggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().statusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().message);
                MessageBox.Show(packetData.GetData<LoginPacketResponse>().message);
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

        private void GetPatientSessionsHandler(DataPacket packetData)
        {
            JObject[] jObjects = packetData.GetData<GetAllPatientsDataResponse>().JObjects;


            Sessions.Clear();
            foreach (JObject jObject in jObjects)
            {
                SessionData session = jObject.ToObject<SessionData>();
                Sessions.Add(session);
            }

            hasSessionResponce = true;
        }

        public void AddDoctorViewmodel(DoctorViewModel viewModel)
        {
            this.DoctorViewModel = viewModel;
        }

        public void AddPastSessionsViewmodel(PastSessionsViewModel viewModel)
        {
            this.PastSessionsViewModel = viewModel;
        }

        private void GetBikeData(DataPacket obj)
        {
            BikeDataPacketDoctor data = obj.GetData<BikeDataPacketDoctor>();

            if (DoctorViewModel.CurrentUser.UserId.Equals(data.id))
            {
                DoctorViewModel.BPM = data.heartRate;
                DoctorViewModel.Speed = data.speed;
                DoctorViewModel.ElapsedTime = data.elapsed;
                DoctorViewModel.Distance = data.distance;
                DoctorViewModel.CurrentUser.speedData.Add(data.speed);
                DoctorViewModel.CurrentUser.bpmData.Add(data.heartRate);
            }
            else
            {
                foreach (Patient patient in _patientList)
                {
                    if (patient.UserId.Equals(data.id))
                    {
                        patient.currentDistance = data.distance;
                        patient.currentSpeed = data.speed;
                        patient.currentElapsedTime = data.elapsed;
                        patient.currentBPM = data.heartRate;
                        patient.speedData.Add(data.speed);
                        patient.bpmData.Add(data.heartRate);
                    }
                }
            }
        }
    }
}