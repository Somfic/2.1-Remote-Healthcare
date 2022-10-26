using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.Common.Socket.Server;

public class SocketServer : ISocket
{
    public static SocketClient Localclient;
    private readonly Log _log = new(typeof(SocketServer));
    private readonly bool _useEncryption;
    private bool _shouldRun;

    public SocketServer(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public TcpListener? Socket { get; private set; }
    public static List<SocketClient> Clients => new();

    public async Task ConnectAsync(string ip, int port)
    {
        _shouldRun = true;

        if (Socket?.Server.Connected == true)
            return;

        try
        {
            _log.Debug($"Starting server on {ip}:{port} ... ({(_useEncryption ? "encrypted" : "unencrypted")})");

            Socket = new TcpListener(IPAddress.Parse(ip), port);
            Socket.Start(0);

            _log.Debug($"Started server on {ip}:{port}");

            // Run on a new thread so that the main thread does not have to wait until a connection is made
            Task.Run(async () => await AcceptConnection());
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"Could not start server on {ip}:{port}");
        }
    }

    public async Task DisconnectAsync()
    {
        _shouldRun = false;
        Socket.Stop();
    }

    private async Task AcceptConnection()
    {
        while (_shouldRun)
            try
            {
                if (Socket == null) throw new NullReferenceException("Listener is null");

                var socket = await Socket.AcceptTcpClientAsync();

                _log.Debug("Socket client connected");

                var client = SocketClient.CreateFromSocket(socket, _useEncryption);
                Localclient = client;
                client.OnMessage += (sender, message) => OnMessage?.Invoke(this, (client, message));
                client.OnDisconnect += (sender, args) => OnClientDisconnected?.Invoke(this, client);
                Clients.Add(client);

                Task.Run(() => { OnClientConnected?.Invoke(this, client); });
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Could not accept new socket client connection");
            }

        _log.Debug("Stopped server");
    }

    public event EventHandler<SocketClient>? OnClientConnected;

    public event EventHandler<SocketClient>? OnClientDisconnected;

    public event EventHandler<(SocketClient client, string message)>? OnMessage;

    public Task BroadcastAsync(dynamic data)
    {
        string json = JsonConvert.SerializeObject(data);
        return BroadcastAsync(json);
    }

    public async Task BroadcastAsync(string text)
    {
        foreach (var client in Clients.Where(x => x.Socket.Connected)) await client.SendAsync(text);
    }
}