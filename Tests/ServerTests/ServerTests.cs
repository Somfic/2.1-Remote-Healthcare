using System.Diagnostics;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.Tests.ServerTests;

public class ServerTests
{
    [Test]
    public async Task Connecting()
    {
        var port = GetRandomPort();

        var server = await CreateNewServer(port);

        var oldClients = server.Clients.Count;

        var client = await ConnectNewClient(port);
        
        var stopwatch = Stopwatch.StartNew();
        while(oldClients == server.Clients.Count && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(oldClients, Is.LessThan(server.Clients.Count));
    }

    [Test]
    public async Task DisconnectingClient()
    {
        var port = GetRandomPort();

        var server = await CreateNewServer(port);
        
        var oldClients = server.Clients.Count;
        
        var client = await ConnectNewClient(port);
        
        while(oldClients == server.Clients.Count) { }
        
        oldClients = server.Clients.Count;

        await client.DisconnectAsync();
        
        var stopwatch = Stopwatch.StartNew();
        while(oldClients == server.Clients.Count && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);
        
        Assert.That(oldClients, Is.GreaterThan(server.Clients.Count));
    }
    
    [Test]
    public async Task ClientToServerMessaging()
    {
        var port = GetRandomPort();
        
        var message = TestHelper.GenerateRandomString(10000);
        
        var server = await CreateNewServer(port);
        var client = await ConnectNewClient(port);

        var receivedMessage = "";
        server.OnMessage += (_, e) => receivedMessage = e.message;
        
        await client.SendAsync(message);

        var stopwatch = Stopwatch.StartNew();
        while(receivedMessage == "" && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(receivedMessage, Is.EqualTo(message));
    }

    [Test]
    public async Task ServerToClientMessaging()
    {
        var port = GetRandomPort();
        
        var message = TestHelper.GenerateRandomString(10000);

        var server = await CreateNewServer(port);
        var client = await ConnectNewClient(port);

        var receivedMessage = "";
        client.OnMessage += (sender, e) => receivedMessage = e;
        
        await server.BroadcastAsync(message);

        var stopwatch = Stopwatch.StartNew();
        while(receivedMessage == "" && stopwatch.ElapsedMilliseconds < 10000)
            await Task.Delay(10);

        Assert.That(receivedMessage, Is.EqualTo(message));
    }

    private async Task<SocketClient> ConnectNewClient(int port)
    {
        SocketClient client = new();
        await client.ConnectAsync("127.0.0.1", port);
        return client;
    }
    
    private async Task<SocketServer> CreateNewServer(int port)
    {
        SocketServer server = new();
        await server.ConnectAsync("127.0.0.1", port);
        return server;
    }
    
    private int GetRandomPort()
    {
        return new Random().Next(10000, 40000);
    }
}