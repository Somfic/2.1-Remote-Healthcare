using System.Net;
using System.Net.Sockets;
using RemoteHealthcare.Common.Logger;

var log = new Log(typeof(Program));

try
{
    log.Information("Starting server");

    var ip = IPEndPoint.Parse("127.0.0.1");

    using Socket listener = new(
        ip.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);

    listener.Bind(ip);
    listener.Listen(100);

    while (true)
    {
        var client = await listener.AcceptAsync();
        Task.Run(async () => await HandleClient(client));
    }
}
catch (Exception ex)
{
    log.Error(ex, "Could not start server");
}

async Task HandleClient(Socket client)
{
    
}