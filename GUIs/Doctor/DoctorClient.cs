using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
    public class DoctorClient : ObservableObject
    {
        public SocketClient Client { get; set; } = new(true);

        private List<string> _connected;

        public List<Patient> PatientList;
        public List<SessionData> Sessions;

        public DoctorViewModel DoctorViewModel;
        public PastSessionsViewModel PastSessionsViewModel;

        private Log _log = new(typeof(DoctorClient));
        public string Password { get; set; }
        public string UserName { get; set; }
        public bool LoggedIn { get; set; }
        private string _userId;

        public bool HasSessionResponce;

        private Dictionary<string, Action<DataPacket>> _callbacks = new();


        public DoctorClient()
        {
            LoggedIn = false;
            HasSessionResponce = false;
            PatientList = new List<Patient>();
            _callbacks = new Dictionary<string, Action<DataPacket>>();
            Sessions = new List<SessionData>();

            //Adds for each key an callback methode in the dictionary 
            _callbacks.Add(OperationCodes.Login, LoginFeature);
            _callbacks.Add(OperationCodes.Users, RequestConnectionsFeature);
            _callbacks.Add(OperationCodes.Chat, ChatHandler);
            _callbacks.Add(OperationCodes.SessionStart, SessionStartHandler);
            _callbacks.Add(OperationCodes.SessionStop, SessionStopHandler);
            _callbacks.Add(OperationCodes.EmergencyStop, EmergencyStopHandler);
            _callbacks.Add(OperationCodes.GetPatientData, GetPatientDataHandler);
            _callbacks.Add(OperationCodes.Bikedata, GetBikeData);
            _callbacks.Add(OperationCodes.GetPatientSesssions, GetPatientSessionsHandler);

            Client.OnMessage += (sender, data) =>
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
                if (LoggedIn)
                {
                    _log.Information("Voer een commando in om naar de server te sturen: \r\n" +
                                     "[BERICHT] [START SESSIE] [STOP SESSIE] [NOODSTOP]");
                    string userCommand = "";

                    if (userCommand.ToLower().Equals("bericht"))
                    {
                        await RequestClients();
                    }
                    else if (userCommand.ToLower().Equals("start") && userCommand.ToLower().Equals("sessie"))
                    {
                        var req = new DataPacket<SessionStartPacketRequest>
                        {
                            OpperationCode = OperationCodes.SessionStart,
                        };
                        _log.Warning($"sending {req.ToJson()}");

                        await Client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Contains(("stop")) && userCommand.ToLower().Contains("Sessie"))
                    {
                        DataPacket<SessionStopPacketRequest> req = new DataPacket<SessionStopPacketRequest>
                        {
                            OpperationCode = OperationCodes.SessionStop,
                        };
                        _log.Warning($"sending {req.ToJson()}");

                        await Client.SendAsync(req);
                    }
                    else if (userCommand.ToLower().Equals("noodstop"))
                    {
                        var req = new DataPacket<EmergencyStopPacket>
                        {
                            OpperationCode = OperationCodes.EmergencyStop,
                        };
                        _log.Warning($"sending {req.ToJson()}");

                        await Client.SendAsync(req);
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
                OpperationCode = OperationCodes.Chat,
                Data = new ChatPacketRequest
                {
                    SenderId = _userId,
                    SenderName = UserName,
                    ReceiverId = target,
                    Message = chatInput
                }
            };

            _log.Warning($"sending {req.ToJson()}");

            await Client.SendAsync(req);
        }

        public async void SetResistance(string target, int res)
        {
            var req = new DataPacket<SetResistancePacket>
            {
                OpperationCode = OperationCodes.SetResistance,
                Data = new SetResistancePacket
                {
                    ReceiverId = target,
                    Resistance = res
                }
            };

            await Client.SendAsync(req);
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

        private async Task RequestClients()
        {
            _log.Debug(_connected.Count.ToString());
            if (_connected != null)
                _connected.Clear();
            _log.Debug(_connected.Count.ToString());
            var req = new DataPacket<ConnectedClientsPacketRequest>
            {
                OpperationCode = OperationCodes.Users,
                Data = new ConnectedClientsPacketRequest
                {
                    Requester = _userId
                }
            };
            _log.Warning($"sending {req.ToJson()}");

            await Client.SendAsync(req);
        }

        public async Task AskForLoginAsync()
        {
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                Data = new LoginPacketRequest
                {
                    UserName = UserName,
                    Password = Password,
                    IsDoctor = true
                }
            };

            _log.Warning($"sending {loginReq.ToJson()}");

            await Client.SendAsync(loginReq);
        }

        public async Task RequestPatientDataAsync()
        {
            DataPacket<GetAllPatientsDataRequest> patientReq = new DataPacket<GetAllPatientsDataRequest>
            {
                OpperationCode = OperationCodes.GetPatientData
            };

            await Client.SendAsync(patientReq);
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
            _log.Information(obj.GetData<SessionStopPacketResponse>().Message);
        }

        //the methode for the session start request
        private void SessionStartHandler(DataPacket obj)
        {
            var sessie = obj.GetData<SessionStartPacketResponse>();

            //Change the GUI with an Alert depends on the outcome of the IF-Statement
            DoctorViewModel.CurrentUserName = (sessie.StatusCode.Equals(StatusCodes.Ok))
                ? DoctorViewModel.CurrentUser.Username
                : "Gekozen Patient is niet online";
        }

        //the methode for the session stop request
        private void SessionStopHandler(DataPacket obj)
        {
            _log.Information(obj.GetData<SessionStopPacketResponse>().Message);
        }
        
        //the methode for printing out the received message
        private void ChatHandler(DataPacket packetData)
        {
            ObservableCollection<string> chats = new();
            foreach (var chatMessage in DoctorViewModel.ChatMessages)
                chats.Add(chatMessage);
            
            chats.Add($"{packetData.GetData<ChatPacketResponse>().SenderName}: {packetData.GetData<ChatPacketResponse>().Message}");
            DoctorViewModel.ChatMessages = chats;
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().StatusCode).Equals(200))
            {
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().ConnectedIds.Split(";").ToList();
                _log.Warning($"RequestConnectionsFeature(): {_connected.Count.ToString()}");

                SendChatAsync();
            }
        }

        //the methode for the login request
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().StatusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().UserId;
                UserName = packetData.GetData<LoginPacketResponse>().UserName;
                _log.Information($"Succesfully logged in to the user: {UserName}; {Password}; {_userId}.");
                LoggedIn = true;
            }
            else
            {
                _log.Error(packetData.GetData<LoginPacketResponse>().StatusCode + "; " +
                           packetData.GetData<LoginPacketResponse>().Message);
                MessageBox.Show(packetData.GetData<LoginPacketResponse>().Message);
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
                PatientList.Add(patient);
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

            HasSessionResponce = true;
        }

        public void AddDoctorViewmodel(DoctorViewModel viewModel)
        {
            DoctorViewModel = viewModel;
        }

        public void AddPastSessionsViewmodel(PastSessionsViewModel viewModel)
        {
            PastSessionsViewModel = viewModel;
        }

        private void GetBikeData(DataPacket obj)
        {
            BikeDataPacketDoctor data = obj.GetData<BikeDataPacketDoctor>();

            if (DoctorViewModel.CurrentUser.UserId.Equals(data.Id))
            {
                DoctorViewModel.Bpm = data.HeartRate;
                DoctorViewModel.Speed = data.Speed;
                DoctorViewModel.ElapsedTime = data.Elapsed;
                DoctorViewModel.Distance = data.Distance;
                DoctorViewModel.CurrentUser.SpeedData.Add(data.Speed);
                DoctorViewModel.CurrentUser.BpmData.Add(data.HeartRate);
            }
            else
            {
                foreach (Patient patient in PatientList)
                {
                    if (patient.UserId.Equals(data.Id))
                    {
                        patient.CurrentDistance = data.Distance;
                        patient.CurrentSpeed = data.Speed;
                        patient.CurrentElapsedTime = data.Elapsed;
                        patient.CurrentBpm = data.HeartRate;
                        patient.SpeedData.Add(data.Speed);
                        patient.BpmData.Add(data.HeartRate);
                    }
                }
            }
        }
    }
}