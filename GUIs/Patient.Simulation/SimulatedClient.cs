using RemoteHealthcare.Client.Data;
using RemoteHealthcare.Client.Data.Providers;
using RemoteHealthcare.Client.Data.Providers.Bike;
using RemoteHealthcare.Client.Data.Providers.Heart;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.GUIs.Patient.Simulation;

public class SimulatedClient
{
    private readonly string _id;
    private readonly Log _log = new(typeof(SimulatedClient));

    private readonly IDataProvider<BikeData> _bikeDataProvider = new SimulationBikeDataProvider();
    private readonly IDataProvider<HeartData> _heartRateDataProvider = new SimulationHeartDataProvider();

    public SimulatedClient(string id)
    {
        _id = id;
        _socket.OnMessage += (sender, e) => _log.Information($"[{_id}] {e}");
    }
    
    private readonly SocketClient _socket = new(true);
    public bool IsConnected => _socket.Socket.Connected;

    public async Task ConnectAsync(string host, int port)
    {
        await _bikeDataProvider.Initialise();
        await _heartRateDataProvider.Initialise();
        
        _log.Debug($"[{_id}] Connecting to server ... "); 
        await _socket.ConnectAsync(host, port);
        _log.Debug($"[{_id}] Connected to server");
    }

    public async Task LoginAsync(string username, string password, bool isDoctor = false)
    {
       var loginReq = new DataPacket<LoginPacketRequest>
        {
            OpperationCode = OperationCodes.LOGIN,
            data = new LoginPacketRequest()
            {
                username = username,
                password = password,
                isDoctor = isDoctor
            }
        };

        await _socket.SendAsync(loginReq);
    }

    public async Task SendBikeData()
    {
        await _bikeDataProvider.ProcessRawData();
        await _heartRateDataProvider.ProcessRawData();
        
        var bikeData = _bikeDataProvider.GetData();
        var heartData = _heartRateDataProvider.GetData();
        
        var dataReq = new DataPacket<BikeDataPacket>
        {
            OpperationCode = OperationCodes.BIKEDATA,

            data = new BikeDataPacket()
            {
                SessionId = _id,
                speed = bikeData.Speed,
                distance = bikeData.Distance,
                heartRate = heartData.HeartRate,
                elapsed = bikeData.TotalElapsed,
                deviceType = bikeData.DeviceType.ToString(),
                id = bikeData.Id
            }
        };
        
        await _socket.SendAsync(dataReq);
    }
}