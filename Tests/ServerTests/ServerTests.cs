using System.Text;
using RemoteHealthcare.Common.Socket;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

namespace CentralServer.Tests;

public class ServerTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task Connecting()
    {
        var server = new SocketServer(false);
        await server.ConnectAsync("127.0.0.1", 12345);

        var connectedClients = 0;

        server.OnClientConnected += (sender, e) => connectedClients++;
        
        var client = new SocketClient(false);
        await client.ConnectAsync("127.0.0.1", 12345);

        await Task.Delay(1000);

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
        await client.SendAsync("Hello world! 123456789");

        await Task.Delay(1000);

        Assert.That(receivedMessage, Is.EqualTo("Hello world! 123456789"));
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
        
        await server.BroadcastAsync("Hello world! 123456789");

        await Task.Delay(1000);

        Assert.That(receivedMessage, Is.EqualTo("Hello world! 123456789"));
    }
    
}