using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Common.Socket.Client;


public class SocketClient
{
    private readonly bool _useEncryption;
    private readonly TcpClient _socket = new();
    private readonly Log _log = new(typeof(SocketClient));

    public SocketClient(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public async Task ConnectAsync(string ip, int port)
    {
        _log.Debug($"Connecting to {ip}:{port}");

        try
        {
            await _socket.ConnectAsync(IPAddress.Parse(ip), port);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Could not connect to {ip}:{port}");
        }

        Task.Run(async () =>
        {
            while (_socket.Connected)
            {
                var text = await SocketHelper.ReadMessage(_socket.GetStream(), _useEncryption);
                OnMessage?.Invoke(this, text);
            }
        });
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
    
    public event EventHandler<string>? OnMessage;
    
    public Task DisconnectAsync()
    {
        _socket.Dispose();
        return Task.CompletedTask;
    }
}