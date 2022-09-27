using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.CentralServer
{
    class Program
    {
        private static TcpListener listener;
        private static List<Client> clients = new List<Client>();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Server!");
            
            listener = new TcpListener(IPAddress.Any, 15243);
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(OnConnect), null);
            Console.ReadLine();
        }

        private static void OnConnect(IAsyncResult ar)
        {
            var tcpClient = listener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Client connected from {tcpClient.Client.RemoteEndPoint}");
            clients.Add(new Client(tcpClient));
            listener.BeginAcceptTcpClient(new AsyncCallback(OnConnect), null);
        }

        internal static void Broadcast(string packet)
        {
            foreach (var client in clients)
            {
                Console.WriteLine("print print print");
                //client.Write(packet);
            }
        }

        internal static void Disconnect(Client client)
        {
            clients.Remove(client);
            Console.WriteLine("Client disconnected");
        }

        internal static void SendToUser(string user, string packet)
        {
            foreach (var client in clients.Where(c => c.UserName == user))
            {
                //TODO:  omzetten naar SendData
                //client.Write(packet);
            }
        }
    }
}