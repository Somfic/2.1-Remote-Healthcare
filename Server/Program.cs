using System.Net;
using System.Net.Sockets;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.Common.Socket.Server;

namespace RemoteHealthcare.CentralServer
{
    class Program
    {
        private static readonly SocketServer _server = new(true);

        static async Task Main(string[] args)
        {
            _server.OnMessage += (sender, e) => OnMessage(e.client, e.message);
            await _server.ConnectAsync("127.0.0.1", 15243);
            Console.ReadLine();
        }

        private static void OnMessage(SocketClient client, string message)
        {
            client.SendAsync(message);
        }

        private static async Task Broadcast(string message)
        {
            await _server.Broadcast(message);
        }
    }
}