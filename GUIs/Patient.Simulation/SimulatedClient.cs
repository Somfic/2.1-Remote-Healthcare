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
                    if (loginResponse.data.StatusCode == StatusCodes.Ok)
                    {
                        OnLogin?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        _log.Error($"[#{Id}] Could not login: '{loginResponse.data.Message}'");
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

    /// <summary>
    /// > Connect to the server and initialise the data providers
    /// </summary>
    /// <param name="host">The IP address of the server.</param>
    /// <param name="port">The port number to connect to.</param>
    public async Task ConnectAsync(string host, int port)
    {
        await _bikeDataProvider.Initialise();
        await _heartRateDataProvider.Initialise();
        
        _log.Debug($"[#{Id}] Connecting to server ... "); 
        await _socket.ConnectAsync(host, port);
        _log.Debug($"[#{Id}] Connected to server");
    }

    /// <summary>
    /// > This function sends a login request to the server
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <param name="password">The password of the user</param>
    /// <param name="isDoctor">If the user is a doctor or not.</param>
    public async Task LoginAsync(string username, string password, bool isDoctor = false)
    {
       var loginReq = new DataPacket<LoginPacketRequest>
        {
            OpperationCode = OperationCodes.Login,
            data = new LoginPacketRequest()
            {
                UserName = username,
                Password = password,
                IsDoctor = isDoctor
            }
        };

        await _socket.SendAsync(loginReq);
    }

    /// <summary>
    /// > Send fake data to the server
    /// </summary>
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
                Speed = bikeData.Speed,
                Distance = bikeData.Distance,
                HeartRate = bikeData.HeartRate,
                Elapsed = bikeData.TotalElapsed,
                DeviceType = bikeData.DeviceType.ToString(),
                Id =  $"Simulation #{Id}",
            }
        };
        
        await _socket.SendAsync(dataReq);
    }

    /// <summary>
    /// It sends a chat message to the server
    /// </summary>
    /// <param name="message">The message to send</param>
    public async Task SendChat(string message)
    {
        var chatReq = new DataPacket<ChatPacketRequest>
        {
            OpperationCode = OperationCodes.Chat,
                            
            data = new ChatPacketRequest()
            {
                SenderId = $"Simulation #{Id}",
                ReceiverId = "Dhr145",
                Message = message
            }
        };
        
        await _socket.SendAsync(chatReq);
    }
}