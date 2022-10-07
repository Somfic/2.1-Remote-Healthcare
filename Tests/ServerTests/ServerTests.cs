using System.Diagnostics;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.Tests.ServerTests;

public class ServerTests
{
    [Test]
    public async Task Connecting()
    {
        const string ip = "127.0.0.1";
        const int port = 12345;
        
        var server = new SocketServer(false);
        await server.ConnectAsync(ip, port);

        var connectedClients = 0;

        server.OnClientConnected += (sender, e) => connectedClients++;
        
        var client = new SocketClient(false);
        await client.ConnectAsync(ip, port);

        var stopwatch = Stopwatch.StartNew();
        while(connectedClients == 0 && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(connectedClients, Is.EqualTo(1));
    }
    
    [Test]
    public async Task ClientToServerMessaging()
    {
        var text = TestHelper.GenerateRandomString();
        const string ip = "127.0.0.1";
        const int port = 12346;
        
        var server = new SocketServer(true);
        await server.ConnectAsync(ip, port);

        var receivedMessage = "";

        server.OnMessage += (sender, e) => receivedMessage = e.message;
        
        var client = new SocketClient(true);
        await client.ConnectAsync(ip, port);
        await client.SendAsync(text);

        var stopwatch = Stopwatch.StartNew();
        while(receivedMessage == "" && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(receivedMessage, Is.EqualTo(text));
    }

    [Test]
    public async Task ServerToClientMessaging()
    {
        var text = TestHelper.GenerateRandomString();
        const string ip = "127.0.0.1";
        const int port = 12347;
        
        var server = new SocketServer(true);
        await server.ConnectAsync(ip, port);

        var client = new SocketClient(true);
        await client.ConnectAsync(ip, port);
        
        var receivedMessage = "";
        client.OnMessage += (sender, e) => receivedMessage = e;
        
        await server.BroadcastAsync(text);

        var stopwatch = Stopwatch.StartNew();
        while(receivedMessage == "" && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(receivedMessage, Is.EqualTo(text));
    }
}