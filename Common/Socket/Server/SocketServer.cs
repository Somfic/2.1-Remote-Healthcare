using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;

namespace RemoteHealthcare.Common.Socket.Server;

public class SocketServer : ISocket
{
    private TcpListener? _socket;
    public bool IsConnected => _socket?.Server.Connected ?? false;
    public EndPoint EndPoint => _socket?.Server.RemoteEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
    
    private readonly Log _log = new(typeof(SocketServer));
    
    private readonly List<SocketClient> _clients = new();
    public IReadOnlyList<SocketClient> Clients => _clients.AsReadOnly();
    
    private readonly bool _useEncryption;
    private bool _shouldRun;

    public SocketServer(bool useEncryption = true)
    {
        _useEncryption = useEncryption;
    }

    public async Task ConnectAsync(string ip, int port)
    {
        _shouldRun = true;
        
        if (_socket?.Server.Connected == true)
            return;

        var attempts = 0;

        while (attempts <= 5)
        {
            attempts++;

            _log.Debug($"Starting server on {ip}:{port} ... ({(_useEncryption ? "encrypted" : "unencrypted")}) (attempt #{attempts})");

            try
            {
                _socket = new TcpListener(IPAddress.Parse(ip), port);
                _socket.Start(0);

                _log.Debug($"Started server on {ip}:{port}");

                // Run on a new thread so that the main thread does not have to wait until a connection is made
                Task.Run(async () => await AcceptConnection());

                break;
            }
            catch (Exception ex)
            {
                if (attempts == 5)
                {
                    _log.Error(ex, $"Could not start server on {ip}:{port}");
                    throw;
                }

                _log.Warning(ex, $"Could not start server on {ip}:{port} ... retrying");
            }

            await Task.Delay(1000);
        }
    }
    
    private async Task AcceptConnection()
    {
        while (_shouldRun)
        {
            try
            { 
                if (_socket == null) throw new NullReferenceException("Listener is null");

                var socket = await _socket.AcceptTcpClientAsync();
                
                _log.Debug($"Socket client connected");
                
                 var client = SocketClient.CreateFromSocket(socket, _useEncryption);
                 
                 client.OnMessage += (sender, message) => OnMessage?.Invoke(this, (client, message));
                 client.OnDisconnect += (sender, args) => 
                 {
                     _log.Debug("Server speaking: Removing client ... ");
                    OnClientDisconnected?.Invoke(this, client);
                    _clients.RemoveAt(_clients.FindIndex(x => x.Id == client.Id));
                 };
                
                 _clients.Add(client);
                 OnClientConnected?.Invoke(this, client);
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Could not accept new socket client connection");
            }
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
        foreach (var client in _clients)
        {
            await client.SendAsync(text);
        }
    }

    public Task DisconnectAsync()
    {
        return Task.Run(async () =>
        {
            _log.Debug("Disconnecting server");
            
            _shouldRun = false;

            while (_socket != null && _clients.Count > 0)
                await Task.Delay(10);
            
            _socket?.Stop();
        });
    }
}