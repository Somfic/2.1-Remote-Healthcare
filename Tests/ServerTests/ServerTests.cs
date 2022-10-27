using System.Diagnostics;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.Tests.ServerTests;

public class ServerTests
{
    private readonly string _message = "{\"OpperationCode\":\"login\",\"data\":{\"username\":\"06111\",\"password\":\"welkom01\",\"isDoctor\":false}}";

    
    [Test]
    public async Task Connecting()
    {
        var server = new SocketServer(false);
        await server.ConnectAsync("127.0.0.1", 12345);

        var connectedClients = 0;

        server.OnClientConnected += (sender, e) => connectedClients++;
        
        var client = new SocketClient(false);
        await client.ConnectAsync("127.0.0.1", 12345);

        var stopwatch = Stopwatch.StartNew();
        while(connectedClients == 0 && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(connectedClients, Is.EqualTo(1));
    }
    
    [Test]
    public async Task ClientToServerMessaging()
    {
        var server = new SocketServer(true);
        await server.ConnectAsync("127.0.0.1", 12346);

        var receivedMessage = "";

        server.OnMessage += (sender, e) => receivedMessage = e.message;
        
        var client = new SocketClient(true);
        await client.ConnectAsync("127.0.0.1", 12346);
        await client.SendAsync(_message);

        var stopwatch = Stopwatch.StartNew();
        while(receivedMessage == "" && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(receivedMessage, Is.EqualTo(_message));
    }

    [Test]
    public async Task ServerToClientMessaging()
    {
        var server = new SocketServer(true);
        await server.ConnectAsync("127.0.0.1", 12347);

        var client = new SocketClient(true);
        await client.ConnectAsync("127.0.0.1", 12347);
        
        var receivedMessage = "";
        client.OnMessage += (sender, e) => receivedMessage = e;
        
        await server.BroadcastAsync(_message);

        var stopwatch = Stopwatch.StartNew();
        while(receivedMessage == "" && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(receivedMessage, Is.EqualTo(_message));
    }
}