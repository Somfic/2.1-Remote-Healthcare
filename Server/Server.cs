using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Client;
using RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server;

public class Server
{
    public static readonly int Port = 15243;

    private readonly SocketServer _server = new(true);
    private readonly Log _log = new(typeof(Server));
    public static PatientData _patientData { get; set; }
    public static DoctorData _doctorData { get; set; }
    public static List<ServerClient> _connectedClients { get; private set; } = new();
    
    public static IReadOnlyList<ServerClient> Clients => _connectedClients.AsReadOnly();


    public async Task StartAsync()
    {
        _patientData = new PatientData();
        _doctorData = new DoctorData();
        
        _server.OnClientConnected += async (sender, e) => await OnClientConnectedAsync(e);
        _server.OnClientDisconnected += async (sender, e) => await OnClientDisconnectedAsync(e);

        await _server.ConnectAsync("127.0.0.1", Port);

        _log.Information($"Server running on port {Port}");
    }

    private async Task OnClientDisconnectedAsync(SocketClient socketClient)
    {
        _connectedClients.Remove(_connectedClients.Find(x => x.Client.Id == socketClient.Id));
    }

    private async Task OnClientConnectedAsync(SocketClient client)
    {
        _log.Information($"Client connected: {client.Socket}");
        _connectedClients.Add(new ServerClient(client));

        _log.Debug("ALLE HUIDIGE TCP-USER ZIJN:");
        foreach (SocketClient user in SocketServer.Clients)
        {
            _log.Debug(user.ToString());
        }

        // _log.Debug("\n");
        //
        // _log.Debug("ALLE HUIDIGE ServerClients-USER ZIJN:");
        // foreach (ServerClient user in _connectedClients)
        // {
        //     _log.Debug(user.ToString());
        // }
    }

    internal static void Disconnect(ServerClient client)
    {
        if (!_connectedClients.Contains(client))
            return;
        Log.Send().Debug("Disconnecting a client now");
        _connectedClients.Remove(client);
    }

    internal static void printUsers()
    {
        Log.Send().Debug("ALLE HUIDIGDE USER NA DE DISCONNECT ZIJN:");
        foreach (SocketClient user in SocketServer.Clients)
        {
            Log.Send().Debug("Socketserver Client:  " + user);
        }
        
        Log.Send().Debug("");
        Log.Send().Debug("ALLE HUIDIGE ServerClients-USER NA DE DISCONNECT ZIJN:");
        
        foreach (ServerClient user in _connectedClients)
        {
            Log.Send().Debug("_connected Clients:  " +user);
        }
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.BroadcastAsync(message);
    }
}