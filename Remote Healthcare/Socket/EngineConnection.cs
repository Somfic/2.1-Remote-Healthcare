using Newtonsoft.Json;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket.Models;

namespace RemoteHealthcare.Socket;

public class EngineConnection
{
    private readonly Socket _socket = new();
    
    public EngineConnection()
    {
        _socket.OnMessage += async (_, json) => await ProcessMessageAsync(json);
    }

    public async Task ConnectAsync()
    {
        await _socket.ConnectAsync("145.48.6.10", 6666);
        await _socket.SendAsync("session/list");
    }

    private async Task ProcessMessageAsync(string json)
    {
        dynamic raw = JsonConvert.DeserializeObject(json) ?? throw new InvalidOperationException("Json was null");
        string id = raw.id;

        switch (id)
        {
            case "session/list":
                var result = JsonConvert.DeserializeObject<SessionListResult>(json);
                // todo: process result
                break;
            
            default:
                Log.Warning($"Unhandled incoming message with id '{id}'");
                break;
        }
    }
}