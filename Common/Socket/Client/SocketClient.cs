using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Common.Socket.Client;


public class SocketClient
{
    private readonly bool _useEncryption;
    public TcpClient Socket { get; private set; } = new();
    private readonly Log _log = new(typeof(SocketClient));

    public SocketClient(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public static SocketClient CreateFromSocket(TcpClient socket, bool useEncryption)
    {
        return new SocketClient(useEncryption) { Socket = socket };
    }

    public async Task ConnectAsync(string ip, int port)
    {
        if (Socket.Connected)
            return;
    
        _log.Debug($"Connecting to {ip}:{port} ({(_useEncryption ? "encrypted" : "unencrypted")})");

        try
        {
            await Socket.ConnectAsync(IPAddress.Parse(ip), port);
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
        return SocketHelper.SendMessage(Socket.GetStream(), text, _useEncryption);
    }

    private async Task Read()
    {
        while (Socket.Connected)
        {
            var text = await SocketHelper.ReadMessage(Socket.GetStream(), _useEncryption);
            OnMessage?.Invoke(this, text);
        }
    }
    
    public event EventHandler<string>? OnMessage;
    
    public Task DisconnectAsync()
    {
        Socket.Dispose();
        return Task.CompletedTask;
    }
}