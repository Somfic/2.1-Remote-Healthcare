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
    public class Client : ObservableObject
    {
        public SocketClient _client { get; set; } = new(true);

        private List<string> _connected;

        public List<Patient> PatientList;
        public List<SessionData> Sessions;

        public DoctorViewModel DoctorViewModel;
        public PastSessionsViewModel PastSessionsViewModel;

        private Log _log = new(typeof(Client));
        public string Password { get; set; }
        public string Username { get; set; }
        public bool LoggedIn { get; set; }
        private string _userId;

        public bool HasSessionResponce;

        private Dictionary<string, Action<DataPacket>> _callbacks;


        public Client()
        {
            LoggedIn = false;
            HasSessionResponce = false;
            PatientList = new List<Patient>();
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

        public async Task SendChatAsync(string? target = null, string? chatInput = null)
        {
            _log.Debug("SendChatAsync(): entered");
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.CHAT,
                data = new ChatPacketRequest()
                {
                    senderId = _userId,
                    senderName = Username,
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

        public async Task AskForLoginAsync()
        {
            DataPacket<LoginPacketRequest> loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.LOGIN,
                data = new LoginPacketRequest()
                {
                    userName = Username,
                    password = Password,
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
            }
        }

        /// <summary>
        /// This function is called when the client receives a response from the server after sending a login request
        /// </summary>
        /// <param name="DataPacket">This is the packet that is received from the server.</param>
        private void LoginFeature(DataPacket packetData)
        {
            _log.Debug($"Responce: {packetData.ToJson()}");
            if (((int)packetData.GetData<LoginPacketResponse>().statusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().userId;
                Username = packetData.GetData<LoginPacketResponse>().userName;
                _log.Information($"Succesfully logged in to the user: {Username}; {Password}; {_userId}.");
                LoggedIn = true;
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
        /// <param name="packetData">This is the object that is sent from the server to the client. It contains the data
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

        /// <summary>
        /// This function is called when the server responds to the client's request for all the sessions of a patient
        /// </summary>
        /// <param name="DataPacket">This is the data that is sent from the server.</param>
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
            this.DoctorViewModel = viewModel;
        }

        public void AddPastSessionsViewmodel(PastSessionsViewModel viewModel)
        {
            this.PastSessionsViewModel = viewModel;
        }

        /// <summary>
        /// This function is called when the server receives a packet of type BikeDataPacketDoctor. It checks if the packet
        /// is for the current user, and if so, it updates the DoctorViewModel with the data. If the packet is not for the
        /// current user, it updates the PatientList with the data
        /// </summary>
        /// <param name="DataPacket">This is the object that is sent from the client to the server. It contains the data
        /// that is sent from the client.</param>
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
                foreach (Patient patient in PatientList)
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