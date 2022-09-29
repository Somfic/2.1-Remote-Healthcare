using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Common.Socket.Client;


public class SocketClient
{
    private readonly bool _useEncryption;
    private TcpClient _socket = new();
    private readonly Log _log = new(typeof(SocketClient));

    public SocketClient(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public static SocketClient CreateFromSocket(TcpClient client, bool useEncryption)
    {
        var socket = new SocketClient(useEncryption);
        socket.SetSocket(client);
        return socket;
    }

    public async Task ConnectAsync(string ip, int port)
    {
        if (_socket.Connected)
            return;
    
        _log.Debug($"Connecting to {ip}:{port} ({(_useEncryption ? "encrypted" : "unencrypted")})");

        try
        {
            await _socket.ConnectAsync(IPAddress.Parse(ip), port);
            _log.Debug($"Connected to {ip}:{port}");
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Could not connect to {ip}:{port}");
        }

        Task.Run(async () => await Read());
    }

    public Task SendAsync(dynamic data)
    {
        string json = JsonConvert.SerializeObject(data);
        return SendAsync(json);
    }
    
    public Task SendAsync(string text)
    {
        return SocketHelper.SendMessage(_socket.GetStream(), text, _useEncryption);
    }

    private async Task Read()
    {
        while (_socket.Connected)
        {
            var text = await SocketHelper.ReadMessage(_socket.GetStream(), _useEncryption);
            OnMessage?.Invoke(this, text);
        }
    }

    private void SetSocket(TcpClient client)
    {
        _socket = client;
    }

    public event EventHandler<string>? OnMessage;
    
    public Task DisconnectAsync()
    {
        _socket.Dispose();
        return Task.CompletedTask;
    }
}