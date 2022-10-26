using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using NetworkEngine.Socket;
using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.GUIs.Patient.ViewModels;

namespace RemoteHealthcare.GUIs.Patient.Client;

public class PatientClient
{
    private readonly Dictionary<string, Action<DataPacket>> _callbacks;
    private string _doctorId;
    private readonly Log _log = new(typeof(PatientClient));
    private bool _sessienRunning;
    private readonly string _sessionId;
    private string _userId;
    public SocketClient Client = new(true);

    public bool LoggedIn;
    public PatientHomepageViewModel P;
    public string Password;
    public string Username;
    public VrConnection VrConnection;

    public PatientClient(VrConnection v)
    {
        LoggedIn = false;
        _callbacks = new Dictionary<string, Action<DataPacket>>();

        //Adds for each key an callback methode in the dictionary 
        _callbacks.Add("login", LoginFeature);
        _callbacks.Add("chat", ChatHandlerAsync);
        _callbacks.Add("session start", SessionStartHandler);
        _callbacks.Add("session stop", SessionStopHandler);
        _callbacks.Add("disconnect", DisconnectHandler);
        _callbacks.Add("set resitance", SetResistanceHandeler);
        _callbacks.Add("emergency stop", EmergencyStopHandler);

        Client.OnMessage += (sender, data) =>
        {
            var packet = JsonConvert.DeserializeObject<DataPacket>(data);
            HandleData(packet);
        };

        _sessionId = DateTime.Now.ToString();
    }

    public async Task PatientLogin()
    {
        var loginReq = new DataPacket<LoginPacketRequest>
        {
            OpperationCode = OperationCodes.Login,
            Data = new LoginPacketRequest
            {
                UserName = Username,
                Password = Password,
                IsDoctor = false
            }
        };
        _log.Error(loginReq.ToJson());

        await Client.SendAsync(loginReq);
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

    private void EmergencyStopHandler(DataPacket obj)
    {
        var data = obj.GetData<EmergencyStopPacket>();
        _log.Critical(data.Message);
    }

    private void SetResistanceHandeler(DataPacket obj)
    {
        VrConnection.SetResistance(obj.GetData<SetResistancePacket>().Resistance);
    }

    //the methode for the disconnect request
    private void DisconnectHandler(DataPacket obj)
    {
    }

    //the methode for the session stop request
    private void SessionStopHandler(DataPacket obj)
    {
        _sessienRunning = false;
        VrConnection.Session = false;
    }

    //the methode for the session start request
    public void SessionStartHandler(DataPacket obj)
    {
        _sessienRunning = true;
        VrConnection.Session = true;
        new Thread(SendBikeDataAsync).Start();
    }

    private void SendBikeDataAsync()
    {
        //if the patient started the sessie the while-loop will be looped till it be false (stop-session)
        while (_sessienRunning)
        {
            var bikedata = VrConnection.GetBikeData();
            var hearthdata = VrConnection.GetHearthData();
            var req = new DataPacket<BikeDataPacket>
            {
                OpperationCode = OperationCodes.Bikedata,

                Data = new BikeDataPacket
                {
                    SessionId = _sessionId,
                    Speed = bikedata.Speed,
                    Distance = bikedata.Distance,
                    HeartRate = hearthdata.HeartRate,
                    Elapsed = bikedata.TotalElapsed,
                    DeviceType = bikedata.DeviceType.ToString(),
                    Id = bikedata.Id
                }
            };

            _log.Information("sending bike data to server");
            Client.SendAsync(req);
            Thread.Sleep(1000);
        }
    }

    //the methode for printing out the received message and sending it to the VR Engine
    private async void ChatHandlerAsync(DataPacket packetData)
    {
        var messageReceived =
            $"{packetData.GetData<ChatPacketResponse>().SenderName}: {packetData.GetData<ChatPacketResponse>().Message}";
        _log.Information(messageReceived);

        var chats = new ObservableCollection<string>();
        foreach (var message in P.Messages)
        {
            chats.Add(message);
        }

        chats.Add(
            $"{packetData.GetData<ChatPacketResponse>().SenderId}: {packetData.GetData<ChatPacketResponse>().Message}");
        P.Messages = chats;


        try
        {
            await VrConnection.Engine.SendTextToChatPannel(messageReceived);
        }
        catch (Exception e)
        {
        }
    }

    //the methode for the login request
    private void LoginFeature(DataPacket packetData)
    {
        _log.Debug($"Responce: {packetData.ToJson()}");

        var statusCode = (int)packetData.GetData<LoginPacketResponse>().StatusCode;
        if (statusCode.Equals(200))
        {
            _userId = packetData.GetData<LoginPacketResponse>().UserId;
            _log.Information($"Succesfully logged in to the user: {Username}; {Password}; {_userId}.");
            LoggedIn = true;
        }
        else
        {
            _log.Error(packetData.GetData<LoginPacketResponse>().StatusCode + "; " +
                       packetData.GetData<LoginPacketResponse>().Message);
        }
    }

    public bool GetLoggedIn()
    {
        return LoggedIn;
    }
}