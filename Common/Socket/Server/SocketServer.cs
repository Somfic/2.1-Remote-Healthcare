using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Common.Socket.Server;

public class SocketServer
{
    private TcpListener? _listener;
    private readonly Log _log = new Log(typeof(SocketServer));
    private readonly List<TcpClient> _clients = new();

    public async Task ConnectAsync(string ip, int port)
    {
        if (_listener?.Server.Connected == true)
            return;

        try
        {
            _log.Debug($"Starting server on {ip}:{port} ... ");

            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _listener.Start();

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
        if (_listener == null)
            throw new NullReferenceException("Listener is null");
        
        var client = await _listener.AcceptTcpClientAsync();

        _log.Debug($"Socket client connected from {client.Client.RemoteEndPoint}");
        
        _clients.Add(client);
        
        Task.Run(() => { OnConnection?.Invoke(this, client); });
    }

    public event EventHandler<TcpClient>? OnConnection;

    public Task Broadcast(dynamic data)
    {
        string json = JsonConvert.SerializeObject(data);
        return Broadcast(json);
    }
    
    public async Task Broadcast(string json)
    {
        foreach (var client in _clients.Where(x => x.Connected))
        {
            await SocketHelper.SendMessage(client.GetStream(), json);
        }
    }
}