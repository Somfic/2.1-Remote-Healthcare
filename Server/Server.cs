using Newtonsoft.Json;
using RemoteHealthcare.CentralServer.Client;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.CentralServer;

public class Server
{
    public static readonly int Port = 15243;
    
    private readonly SocketServer _server = new(true);
    private readonly Log _log = new(typeof(Server));
    
    private static List<ServerClient> _connectedClients = new List<ServerClient>();


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
    }
    
    internal static void Disconnect(ServerClient client)
    {
        if (_connectedClients.Contains(client))
        {
            Console.WriteLine("bestaat");
        }
        else
        {
            Console.WriteLine("bestaat niet");
        }
        //_connectedClients.Remove(client);
       // Console.WriteLine("Client disconnected");
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.Broadcast(message);
    }
}