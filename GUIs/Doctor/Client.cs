using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MvvmHelpers;
using Newtonsoft.Json;
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
            _callbacks.Add(OperationCodes.Login, LoginFeature);
            _callbacks.Add(OperationCodes.Users, RequestConnectionsFeature);
            _callbacks.Add(OperationCodes.Chat, ChatHandler);
            _callbacks.Add(OperationCodes.SessionStart, SessionStartHandler);
            _callbacks.Add(OperationCodes.SessionStop, SessionStopHandler);
            _callbacks.Add(OperationCodes.EmergencyStop, EmergencyStopHandler);
            _callbacks.Add(OperationCodes.GetPatientData, GetPatientDataHandler);
            _callbacks.Add(OperationCodes.Bikedata, GetBikeData);
            _callbacks.Add(OperationCodes.GetPatientSesssions, GetPatientSessionsHandler);

            _client.OnMessage += (sender, data) =>
            {
                var packet = JsonConvert.DeserializeObject<DataPacket>(data);
                HandleData(packet);
            };
        }

        public async Task SendChatAsync(string? target = null, string? chatInput = null)
        {
            var req = new DataPacket<ChatPacketRequest>
            {
                OpperationCode = OperationCodes.Chat,
                data = new ChatPacketRequest()
                {
                    SenderId = _userId,
                    SenderName = Username,
                    ReceiverId = target,
                    Message = chatInput
                }
            };

            await _client.SendAsync(req);
        }

        public async void SetResistance(string target, int res)
        {
            var req = new DataPacket<SetResistancePacket>
            {
                OpperationCode = OperationCodes.SetResistance,
                data = new SetResistancePacket()
                {
                    ReceiverId = target,
                    Resistance = res
                }
            };

            await _client.SendAsync(req);
        }

        public async Task AskForLoginAsync()
        {
            var loginReq = new DataPacket<LoginPacketRequest>
            {
                OpperationCode = OperationCodes.Login,
                data = new LoginPacketRequest()
                {
                    UserName = Username,
                    Password = Password,
                    IsDoctor = true
                }
            };

            await _client.SendAsync(loginReq);
        }

        public async Task RequestPatientDataAsync()
        {
            var patientReq = new DataPacket<GetAllPatientsDataRequest>
            {
                OpperationCode = OperationCodes.GetPatientData
            };

            await _client.SendAsync(patientReq);
        }

        //this methode will get the right methode that will be used for the response from the server
        public void HandleData(DataPacket packet)
        {
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
            foreach (var chatMessage in DoctorViewModel._chatMessages)
                chats.Add(chatMessage);
            
            chats.Add($"{packetData.GetData<ChatPacketResponse>().SenderName}: {packetData.GetData<ChatPacketResponse>().Message}");
            DoctorViewModel.ChatMessages = chats;
        }

        private void RequestConnectionsFeature(DataPacket packetData)
        {
            if (((int)packetData.GetData<ConnectedClientsPacketResponse>().StatusCode).Equals(200))
            {
                _connected = packetData.GetData<ConnectedClientsPacketResponse>().ConnectedIds.Split(";").ToList();
            }
        }

        /// <summary>
        /// This function is called when the client receives a response from the server after sending a login request
        /// </summary>
        /// <param name="DataPacket">This is the packet that is received from the server.</param>
        private void LoginFeature(DataPacket packetData)
        {
            if (((int)packetData.GetData<LoginPacketResponse>().StatusCode).Equals(200))
            {
                _userId = packetData.GetData<LoginPacketResponse>().UserId;
                Username = packetData.GetData<LoginPacketResponse>().UserName;
                _log.Information($"Succesfully logged in to the user: {Username}; {Password}; {_userId}.");
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
        /// <param name="packetData">This is the object that is sent from the server to the client. It contains the data
        /// that is sent from the server.</param>
        private void GetPatientDataHandler(DataPacket packetData)
        {
            var jObjects = packetData.GetData<GetAllPatientsDataResponse>().JObjects;

            foreach (var jObject in jObjects)
            {
                var patient = jObject.ToObject<Patient>();
                PatientList.Add(patient);
            }
        }

        /// <summary>
        /// This function is called when the server responds to the client's request for all the sessions of a patient
        /// </summary>
        /// <param name="DataPacket">This is the data that is sent from the server.</param>
        private void GetPatientSessionsHandler(DataPacket packetData)
        {
            var jObjects = packetData.GetData<GetAllPatientsDataResponse>().JObjects;


            Sessions.Clear();
            foreach (var jObject in jObjects)
            {
                var session = jObject.ToObject<SessionData>();
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
            var data = obj.GetData<BikeDataPacketDoctor>();

            if (DoctorViewModel.CurrentUser.UserId.Equals(data.Id))
            {
                DoctorViewModel.BPM = data.HeartRate;
                DoctorViewModel.Speed = data.Speed;
                DoctorViewModel.ElapsedTime = data.Elapsed;
                DoctorViewModel.Distance = data.Distance;
                DoctorViewModel.CurrentUser.SpeedData.Add(data.Speed);
                DoctorViewModel.CurrentUser.BpmData.Add(data.HeartRate);
            }
            else
            {
                foreach (var patient in PatientList)
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