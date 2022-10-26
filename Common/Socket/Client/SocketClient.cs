using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.Common.Socket.Client;

public class SocketClient : ISocket
{
    private readonly Log _log = new(typeof(SocketClient));
    private readonly bool _useEncryption;
    public Dictionary<string, Action<DataPacket>> Callbacks;

    public SocketClient(bool useEncryption)
    {
        _useEncryption = useEncryption;
        Callbacks = new Dictionary<string, Action<DataPacket>>();
    }

    public TcpClient Socket { get; private set; } = new();

    public Guid Id { get; } = Guid.NewGuid();

    public async Task ConnectAsync(string ip, int port)
    {
        if (Socket.Connected)
            return;

        var attempts = 0;

        while (attempts < 5)
        {
            attempts++;

            _log.Debug(
                $"Connecting to {ip}:{port} ({(_useEncryption ? "encrypted" : "unencrypted")}) (attempt #{attempts})");

            try
            {
                await Socket.ConnectAsync(IPAddress.Parse(ip), port);
                _log.Debug($"Connected to {ip}:{port}");
                Read();
                break;
            }
            catch (Exception ex)
            {
                Socket.Close();
                Socket = new TcpClient();

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

    public Task DisconnectAsync()
    {
        OnDisconnect?.Invoke(this, EventArgs.Empty);
        SocketServer.Clients.Remove(SocketServer.Localclient);
        Socket.Dispose();

        return Task.CompletedTask;
    }

    public static SocketClient CreateFromSocket(TcpClient socket, bool useEncryption)
    {
        var client = new SocketClient(useEncryption) { Socket = socket };
        client.Read();
        return client;
    }

    public Task SendAsync(dynamic data)
    {
        string json = JsonConvert.SerializeObject(data);
        return SendAsync(json);
    }

    public Task SendAsync<T>(DataPacket<T> packet, Action<DataPacket> callback) where T : DAbstract
    {
        Callbacks.Add(packet.OpperationCode, callback);

        var json = JsonConvert.SerializeObject(packet);
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
            throw;
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
                    _log.Error(ex, "Client disconnected");
                    await DisconnectAsync();
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(text))
                        OnMessage?.Invoke(this, text);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"Error while handling message: '{text}'");
                }
            }

            _log.Debug("Client disconnected");
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        });
    }

    public event EventHandler<string>? OnMessage;

    public event EventHandler? OnDisconnect;

    public override string ToString()
    {
        return
            $"IP Adress: {((IPEndPoint)Socket.Client.RemoteEndPoint).Address}; Port: {((IPEndPoint)Socket.Client.RemoteEndPoint).Port}";
    }
}