using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.Common.Socket.Server;

public class SocketServer
{
    public TcpListener? Socket { get; private set; }
    private readonly Log _log = new(typeof(SocketServer));
    private readonly List<SocketClient> _clients = new();
    private readonly bool _useEncryption;
    
    public SocketServer(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public async Task ConnectAsync(string ip, int port)
    {
        if (Socket?.Server.Connected == true)
            return;

        try
        {
            _log.Debug($"Starting server on {ip}:{port} ... ({(_useEncryption ? "encrypted" : "unencrypted")})");

            Socket = new TcpListener(IPAddress.Parse(ip), port);
            Socket.Start();

            _log.Debug($"Started server on {ip}:{port}");

            // Run on a new thread so that the main thread does not have to wait until a connection is made
            Task.Run(async () => await AcceptConnection());
        }
        catch (Exception ex)
        {
            _log.Warning(ex,$"Could not start server on {ip}:{port}");
        }
    }

    private async Task AcceptConnection()
    {
        while (Socket?.Server.Connected == true)
        {
            try
            {
                if (Socket == null) throw new NullReferenceException("Listener is null");

                var socket = await Socket.AcceptTcpClientAsync();
                var client = SocketClient.CreateFromSocket(socket, _useEncryption);

                client.OnMessage += (sender, message) => OnMessage?.Invoke(this, (client, message));

                _log.Debug($"Socket client connected");

                _clients.Add(client);

                Task.Run(() => { OnClientConnected?.Invoke(this, client); });
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Could not accept socket client");
            }
        }
    }

    public event EventHandler<SocketClient>? OnClientConnected;

    public event EventHandler<(SocketClient client, string message)>? OnMessage; 

    public Task Broadcast(dynamic data)
    {
        string json = JsonConvert.SerializeObject(data);
        return Broadcast(json);
    }
    
    public async Task Broadcast(string json)
    {
        foreach (var client in _clients.Where(x => x.Socket.Connected))
        {
            await client.SendAsync(json);
        }
    }
}