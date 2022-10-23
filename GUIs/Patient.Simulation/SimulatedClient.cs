using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.GUIs.Patient.Simulation;

public class SimulatedClient
{
    private readonly string _id;
    private readonly Log _log = new(typeof(SimulatedClient));

    public SimulatedClient(string id)
    {
        _id = id;
    }
    
    private readonly SocketClient _socket = new(true);

    public async Task ConnectAsync(string host, int port)
    {
        _log.Debug($"[{_id}] Connecting to server ... "); 
        await _socket.ConnectAsync(host, port);
        _log.Debug($"[{_id}] Connected to server");
        
        
    }

    public async Task LoginAsync()
    {
       var loginReq = new DataPacket<LoginPacketRequest>
        {
            OpperationCode = OperationCodes.LOGIN,
            data = new LoginPacketRequest()
            {
                username = "123",
                password = "123",
                isDoctor = false
            }
        };

        await _socket.SendAsync(loginReq);
    }
}