using Newtonsoft.Json;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;
using RemoteHealthcare.Server.ServerClient;

namespace RemoteHealthcare.CentralServer;

public class Server
{
    public static readonly int Port = 15243;
    
    private readonly SocketServer _server = new(true);
    private readonly Log _log = new(typeof(Server));

    public async Task StartAsync()
    {
        _server.OnClientConnected += async (sender, e) => await OnClientConnected(e);
        
        await _server.ConnectAsync("127.0.0.1", Port);
        
        _log.Information($"Server running on port {Port}");
    }
    
    
    private async Task OnClientConnected(SocketClient client)
    {
        _log.Information($"Client connected: {client.Socket}");
        ServerClient serverClient = new ServerClient(client);
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.Broadcast(message);
    }
}