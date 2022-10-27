using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server;

public class Server
{
    private readonly int _port = 15243;

    private readonly SocketServer _server = new(true);
    private readonly Log _log = new(typeof(Server));
    public static PatientData PatientData { get; set; }
    public static DoctorData DoctorData { get; set; }
    public static List<ServerClient> ConnectedClients { get; private set; } = new();


    /// <summary>
    /// The server is listening for connections on port `Port` and when a client connects, the server will call the
    /// `OnClientConnectedAsync` function
    /// </summary>
    public async Task StartAsync()
    {
        _server.OnClientConnected += async (sender, e) => await OnClientConnectedAsync(e);
        _server.OnClientDisconnected += async (sender, e) => await OnClientDisconnectedAsync(e);

        await _server.ConnectAsync("127.0.0.1", _port);
        
        PatientData = new PatientData();
        DoctorData = new DoctorData();
        _log.Information($"Server running on port {_port}");
    }

    private async Task OnClientDisconnectedAsync(SocketClient socketClient)
    {
        ConnectedClients.Remove(ConnectedClients.Find(x => x.Client.Id == socketClient.Id));
    }

    private async Task OnClientConnectedAsync(SocketClient client)
    {
        _log.Information($"Client connected: {client.Socket}");
        ConnectedClients.Add(new ServerClient(client));
    }

    internal static void Disconnect(ServerClient client)
    {
        if (!ConnectedClients.Contains(client))
            return;
        Log.Send().Information("Disconnecting a client now");
        ConnectedClients.Remove(client);
    }
}