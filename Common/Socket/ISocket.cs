namespace RemoteHealthcare.Common.Socket;

public interface ISocket
{
    Task ConnectAsync(string ip, int port);

    Task DisconnectAsync();
}