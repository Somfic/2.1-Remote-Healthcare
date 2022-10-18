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
    public static List<ServerClient> ConnectedClients { get; private set; } = new List<ServerClient>();
    
    public static IReadOnlyList<ServerClient> Clients => ConnectedClients.AsReadOnly();


    public async Task StartAsync()
    {
        PatientData = new PatientData();
        DoctorData = new DoctorData();
        
        _server.OnClientConnected += async (sender, e) => await OnClientConnectedAsync(e);
        _server.OnClientDisconnected += async (sender, e) => await OnClientDisconnectedAsync(e);

        await _server.ConnectAsync("127.0.0.1", Port);

        _log.Information($"Server running on port {Port}");
    }

    private async Task OnClientDisconnectedAsync(SocketClient socketClient)
    {
        ConnectedClients.Remove(ConnectedClients.Find(x => x.Client.Id == socketClient.Id));
    }

    private async Task OnClientConnectedAsync(SocketClient client)
    {
        _log.Information($"Client connected: {client.EndPoint}");
        ConnectedClients.Add(new ServerClient(client));

        Console.WriteLine("ALLE HUIDIGE TCP-USER ZIJN:");
        foreach (SocketClient user in _server.Clients)
        {
            _log.Debug(user.ToString());
        }

        // Console.WriteLine("\n");
        //
        // Console.WriteLine("ALLE HUIDIGE ServerClients-USER ZIJN:");
        // foreach (ServerClient user in _connectedClients)
        // {
        //     _log.Debug(user.ToString());
        // }
    }

    internal static void Disconnect(ServerClient client)
    {
        if (!ConnectedClients.Contains(client))
            return;
        Console.WriteLine("Disconnecting a client now");
        ConnectedClients.Remove(client);
    }

    internal void PrintUsers()
    {
        Console.WriteLine("ALLE HUIDIGDE USER NA DE DISCONNECT ZIJN:");
        foreach (SocketClient user in _server.Clients)
        {
            Console.WriteLine("Socketserver Client:  " + user);
        }
        
        Console.WriteLine(" \n ");
        
        Console.WriteLine("ALLE HUIDIGE ServerClients-USER NA DE DISCONNECT ZIJN:");
        
        foreach (ServerClient user in ConnectedClients)
        {
            Console.WriteLine("_connected Clients:  " +user);
        }
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.BroadcastAsync(message);
    }
}