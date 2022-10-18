using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

var server = new SocketServer();
await server.ConnectAsync("127.0.0.1", 12345);

var client = new SocketClient();
await client.ConnectAsync("127.0.0.1", 12345);

await Task.Delay(1000);

await client.DisconnectAsync();

await Task.Delay(-1);