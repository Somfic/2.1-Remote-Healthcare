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
    public static PatientData PatientData { get; set; }
    public static DoctorData DoctorData { get; set; }
    public static List<ServerClient> ConnectedClients { get; private set; } = new();
    
    public static IReadOnlyList<ServerClient> Clients => ConnectedClients.AsReadOnly();


    public async Task StartAsync()
    {
        _server.OnClientConnected += async (sender, e) => await OnClientConnectedAsync(e);
        _server.OnClientDisconnected += async (sender, e) => await OnClientDisconnectedAsync(e);

        await _server.ConnectAsync("127.0.0.1", Port);
        
        PatientData = new PatientData();
        DoctorData = new DoctorData();
        _log.Information($"Server running on port {Port}");
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
        Log.Send().Debug("Disconnecting a client now");
        ConnectedClients.Remove(client);
    }

    internal static void PrintUsers()
    {
        Log.Send().Debug("ALLE HUIDIGDE USER NA DE DISCONNECT ZIJN:");
        foreach (SocketClient user in SocketServer.Clients)
        {
            Log.Send().Debug("Socketserver Client:  " + user);
        }
        
        Log.Send().Debug("");
        Log.Send().Debug("ALLE HUIDIGE ServerClients-USER NA DE DISCONNECT ZIJN:");
        
        foreach (ServerClient user in ConnectedClients)
        {
            Log.Send().Debug("_connected Clients:  " +user);
        }
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.BroadcastAsync(message);
    }
}