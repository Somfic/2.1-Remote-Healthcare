using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.Client;

namespace RemoteHealthcare.Server;

public class Server
{
    public static readonly int Port = 15243;
    
    private readonly SocketServer _server = new(true);
    private readonly Log _log = new(typeof(Server));
    private static List<ServerClient> _connectedClients { get; set; } = new List<ServerClient>() ;

    public async Task StartAsync()
    {
        _server.OnClientConnected += async (sender, e) => await OnClientConnected(e);
        
        await _server.ConnectAsync("127.0.0.1", Port);
        
        _log.Information($"Server running on port {Port}");
    }
    
    private async Task OnClientConnected(SocketClient client)
    {
        _log.Information($"Client connected: {client.Socket}");
        _connectedClients.Add(new ServerClient(client));

        Console.WriteLine("ALLE INGELOGDE USER ZIJN:");
        foreach (SocketClient user in SocketServer.Clients)
        {
            Console.WriteLine(user);
        }
    }
    
    internal static void Disconnect(ServerClient client)
    {
        if (!_connectedClients.Contains(client)) 
        {
            return;
        }
        _connectedClients.Remove(client);
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.BroadcastAsync(message);
    }
}