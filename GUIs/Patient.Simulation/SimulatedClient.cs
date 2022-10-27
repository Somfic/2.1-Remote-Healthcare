using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Data;
using RemoteHealthcare.Common.Data.Providers;
using RemoteHealthcare.Common.Data.Providers.Bike;
using RemoteHealthcare.Common.Data.Providers.Heart;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.GUIs.Patient.Simulation;

public class SimulatedClient
{
    public readonly int Id;
    private readonly Log _log = new(typeof(SimulatedClient));

    private readonly IDataProvider<BikeData> _bikeDataProvider = new SimulationBikeDataProvider();
    private readonly IDataProvider<HeartData> _heartRateDataProvider = new SimulationHeartDataProvider();

    public SimulatedClient(int id)
    {
        Id = id;
        _socket.OnMessage += (sender, json) =>
        {
            var dataPacket = JsonConvert.DeserializeObject<DataPacket>(json);

            switch (dataPacket.OpperationCode)
            {
                case "login":
                    var loginResponse = JsonConvert.DeserializeObject<DataPacket<LoginPacketResponse>>(json);
                    if (loginResponse.data.statusCode == StatusCodes.OK)
                    {
                        OnLogin?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        _log.Error($"[#{Id}] Could not login: '{loginResponse.data.message}'");
                        _socket.DisconnectAsync();
                    }
                    break;
                
                default:
                    _log.Warning($"[#{Id}] Unhandled incoming request: '{dataPacket.OpperationCode}': {json}");
                    break;
            }
        };
    }
    
    private readonly SocketClient _socket = new(true);
    public bool IsConnected => _socket.Socket.Connected;

    public event EventHandler OnLogin;

    public async Task ConnectAsync(string host, int port)
    {
        await _bikeDataProvider.Initialise();
        await _heartRateDataProvider.Initialise();
        
        _log.Debug($"[#{Id}] Connecting to server ... "); 
        await _socket.ConnectAsync(host, port);
        _log.Debug($"[#{Id}] Connected to server");
    }

    public async Task LoginAsync(string username, string password, bool isDoctor = false)
    {
       var loginReq = new DataPacket<LoginPacketRequest>
        {
            OpperationCode = OperationCodes.Login,
            data = new LoginPacketRequest()
            {
                userName = username,
                password = password,
                isDoctor = isDoctor
            }
        };

        await _socket.SendAsync(loginReq);
    }

    public async Task SendBikeData()
    {
        _log.Debug($"[#{Id}] Sending fake data");
        
        await _bikeDataProvider.ProcessRawData();
        await _heartRateDataProvider.ProcessRawData();
        
        var bikeData = _bikeDataProvider.GetData();
        
        var dataReq = new DataPacket<BikeDataPacket>
        {
            OpperationCode = OperationCodes.Bikedata,

            data = new BikeDataPacket()
            {
                SessionId = $"Simulation #{Id}",
                speed = bikeData.Speed,
                distance = bikeData.Distance,
                heartRate = bikeData.HeartRate,
                elapsed = bikeData.TotalElapsed,
                deviceType = bikeData.DeviceType.ToString(),
                id =  $"Simulation #{Id}",
            }
        };
        
        await _socket.SendAsync(dataReq);
    }

    public async Task SendChat(string message)
    {
        var chatReq = new DataPacket<ChatPacketRequest>
        {
            OpperationCode = OperationCodes.Chat,
                            
            data = new ChatPacketRequest()
            {
                senderId = $"Simulation #{Id}",
                receiverId = "Dhr145",
                message = message
            }
        };
        
        await _socket.SendAsync(chatReq);
    }
}