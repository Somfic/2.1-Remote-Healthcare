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

        await _server.ConnectAsync("127.0.0.1", Port);

        _log.Information($"Server running on port {Port}");
    }

    private async Task OnClientConnectedAsync(SocketClient client)
    {
        _log.Information($"Client connected: {client.Socket}");
        _connectedClients.Add(new ServerClient(client));

        _log.Information("ALLE HUIDIGE TCP-USER ZIJN:");
        foreach (SocketClient user in SocketServer.Clients)
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
        if (!_connectedClients.Contains(client))
            return;
        Console.WriteLine("!!!Disconnecting a client now!!!");
        _connectedClients.Remove(client);
    }

    internal static void printUsers()
    {
        Console.WriteLine("ALLE HUIDIGDE USER NA DE DISCONNECT ZIJN:");
        foreach (SocketClient user in SocketServer.Clients)
        {
            Console.WriteLine("Socketserver Client:  " + user);
        }
        
        Console.WriteLine(" \n ");
        
        Console.WriteLine("ALLE HUIDIGE ServerClients-USER NA DE DISCONNECT ZIJN:");
        
        foreach (ServerClient user in _connectedClients)
        {
            Console.WriteLine("_connected Clients:  " +user);
        }
    }

    private async Task BroadcastAsync(string message)
    {
        await _server.BroadcastAsync(message);
    }
}