using Newtonsoft.Json;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket.Models;
using RemoteHealthcare.Socket.Models.Response;

namespace RemoteHealthcare.Socket;

public class EngineConnection
{
    private readonly Socket _socket = new();
    private readonly Log _log = new(typeof(EngineConnection));
    private (string user, string uid)[]? _clients;
    
    private string _tunnelId;
    private string _userId;

    public EngineConnection()
    {
        _socket.OnMessage += async (_, json) => await ProcessMessageAsync(json);
    }

    public async Task<string[]> FindAvailableUsersAsync()
    {
        _clients = null;
        
        await CreateConnectionAsync();
        await _socket.SendAsync("session/list");

        while (true)
        {
            if(_clients != null)
                return _clients.Select(x => x.user).ToArray();

            await Task.Delay(50);
        }
    }

    public async Task ConnectAsync(string user)
    {
        await CreateConnectionAsync();
        await FindAvailableUsersAsync();
        
        if (!_clients.Any(x => x.user.ToLower().Contains(user.ToLower())))
        {
            _log.Warning($"User {user} could not be found. Available users: {string.Join(", ", _clients.Select(x => x.user))}");
            throw new ArgumentException("User could not be found");
        }

        var foundUser = _clients.First(x => x.user.ToLower().Contains(user.ToLower()));
        _userId = foundUser.uid;
        _log.Debug($"Connecting to {foundUser.user} ({foundUser.uid}) ... ");
        
        await _socket.SendAsync($"tunnel/create", new { session = _userId });
    }

    private async Task ProcessMessageAsync(string json)
    {
        dynamic raw = JsonConvert.DeserializeObject(json) ?? throw new InvalidOperationException("Json was null");
        string id = raw.id;
        switch (id)
        {
            case "session/list":
            {
                var result = JsonConvert.DeserializeObject<DataResponses<SessionList>>(json);
                _clients = result.Data.Select(x => (user: $"{x.Client.Host}/{x.Client.User}", uid: x.Id)).ToArray();
                _log.Debug($"Found {_clients.Length} clients: {string.Join(", ", _clients.Select(x => x.user))}");
                break;
            }

            case "tunnel/create":
            {
                var result = JsonConvert.DeserializeObject<DataResponse<TunnelCreate>>(json);
                _tunnelId = result.Data.Id;
                var user = _clients?.First(x => x.uid == _userId).user;
                _log.Information($"Connected to {user}");
                break;
            }

            default:
            {
                _log.Warning($"Unhandled incoming message with id '{id}'");
                Console.WriteLine(json);
                break;
            }
        }
    }

    private async Task CreateConnectionAsync()
    {
        await _socket.ConnectAsync("145.48.6.10", 6666);
    }
}