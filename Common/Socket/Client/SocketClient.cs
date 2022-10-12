using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.Common.Socket.Client;

public class SocketClient : ISocket
{
    private readonly bool _useEncryption;
    public TcpClient Socket { get; private set; } = new();
    
    public Guid Id { get; } = Guid.NewGuid();
    
    private readonly Log _log = new(typeof(SocketClient));

    public SocketClient(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public static SocketClient CreateFromSocket(TcpClient socket, bool useEncryption)
    {
        var client = new SocketClient(useEncryption) { Socket = socket };
        client.Read();
        return client;
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
            Read();
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Could not connect to {ip}:{port}");
        }
    }

    public Task SendAsync(dynamic data)
    {
        string json = JsonConvert.SerializeObject(data);
        return SendAsync(json);
    }
    
    public Task SendAsync(string text)
    {
        try
        {
            return SocketHelper.SendMessage(Socket.GetStream(), text, _useEncryption);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Could not send message: '{text}'");
            return Task.CompletedTask;
        }
    }

  
    private void Read()
    {
        Task.Run(async () =>
        {
            while (Socket.Connected)
            {
                var text = string.Empty;
                
                try
                {
                    text = await SocketHelper.ReadMessage(Socket.GetStream(), _useEncryption);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Error while trying to read message from socket");
                }
                
                try
                {
                    if(!string.IsNullOrWhiteSpace(text))
                        OnMessage?.Invoke(this, text);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"Error while handling message: '{text}'");
                }
            }

            _log.Debug($"Client disconnected");
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        });
    }
    
    public event EventHandler<string>? OnMessage;
    
    public event EventHandler? OnDisconnect;
    
    public Task DisconnectAsync()
    {
        SocketServer._clients.Remove(SocketServer.Localclient); 
        Socket.Dispose();
        
        return Task.CompletedTask;
    }

    public override string ToString()
    {
        return $"IP Adress: {((IPEndPoint)Socket.Client.RemoteEndPoint).Address}; Port: {((IPEndPoint)Socket.Client.RemoteEndPoint).Port}";
    }
}