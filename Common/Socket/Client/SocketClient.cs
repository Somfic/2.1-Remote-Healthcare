using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Common.Socket.Client;

public class SocketClient : ISocket
{
    private readonly bool _useEncryption;
    
    private TcpClient _socket = new(); 
    
    public bool IsConnected => _socket.Connected;
    public EndPoint EndPoint => _socket.Client.RemoteEndPoint;
    
    public Guid Id { get; } = Guid.NewGuid();
    
    private readonly Log _log = new(typeof(SocketClient));

    public SocketClient(bool useEncryption = true)
    {
        _useEncryption = useEncryption;
    }

    public static SocketClient CreateFromSocket(TcpClient socket, bool useEncryption = true)
    {
        var client = new SocketClient(useEncryption) { _socket = socket };
        client.Read();
        return client;
    } 

    public async Task ConnectAsync(string ip, int port)
    {
        if (_socket.Connected)
            return;

        var attempts = 0;

        while (attempts <= 5)
        {
            attempts++;
            
            _log.Debug($"Connecting to {ip}:{port} ({(_useEncryption ? "encrypted" : "unencrypted")}) (attempt #{attempts})");

            try
            {
                await _socket.ConnectAsync(IPAddress.Parse(ip), port);
                _log.Debug($"Connected to {ip}:{port}");
                Read();
                break;
            }
            catch (Exception ex)
            {
                _socket.Close();
                _socket = new TcpClient();

                if (attempts == 5)
                {
                    _log.Error(ex, $"Could not connect to {ip}:{port}");
                    throw;
                }

                _log.Warning(ex, $"Could not connect to {ip}:{port} ... retrying");
            }
            
            await Task.Delay(1000);
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
            return SocketHelper.SendMessage(_socket.GetStream(), text, _useEncryption);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Could not send message: '{text}'");
            throw;
        }
    }

  
    private void Read()
    {
        Task.Run(async () =>
        {
            while (_socket.Connected)
            {
                var text = string.Empty;
                
                try
                {
                    text = await SocketHelper.ReadMessage(_socket.GetStream(), _useEncryption);
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

            _log.Debug($"Firing disconnect event");
            OnDisconnect.Invoke(this, EventArgs.Empty);
        });
    }
    
    public event EventHandler<string> OnMessage;
    
    public event EventHandler OnDisconnect;
    
    public Task DisconnectAsync()
    {
        return Task.Run(async () =>
        {
            _log.Debug("Disconnecting client");
            
            _socket.Close();

            while (_socket.Connected)
                await Task.Delay(10);
            
            _log.Debug("Client disconnected");
        });
    }

    public override string ToString()
    {
        return $"SocketClient({((IPEndPoint)_socket.Client.RemoteEndPoint).Address}:{((IPEndPoint)_socket.Client.RemoteEndPoint).Port})";
    }
}