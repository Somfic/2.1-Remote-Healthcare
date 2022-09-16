using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    private string _groundPlaneId;

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

    public async Task ConnectAsync(string user, string? password = null)
    {
        await CreateConnectionAsync();
        await FindAvailableUsersAsync();

        if (_clients == null)
        {   
            _log.Warning("No clients are available");
            throw new Exception("No clients were found");
        }
        
        if (!_clients.Any(x => x.user.ToLower().Contains(user.ToLower())))
        {
            _log.Warning($"User '{user}' could not be found. Available users: {string.Join(", ", _clients.Select(x => x.user))}");
            throw new ArgumentException("User could not be found");
        }

        var foundUser = _clients.First(x => x.user.ToLower().Contains(user.ToLower()));
        _userId = foundUser.uid;
        _log.Debug($"Connecting to {foundUser.user} ({foundUser.uid}) ... ");
        
        await _socket.SendAsync("tunnel/create", new { session = _userId, key = password });
        
        Thread.Sleep(2000);
        
        await _socket.SendTerrain(_tunnelId);
        await _socket.AddNode(_tunnelId);
        
        Thread.Sleep(1000);
        
        _log.Debug("Getting scene");
        await _socket.GetScene(_tunnelId);
    }

    private async Task ProcessMessageAsync(string json)
    {
        try
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
                    var user = _clients?.First(x => x.uid == _userId).user;

                    if (result.Data.Status != "ok")
                    {
                        _log.Warning($"Could not establish tunnel connection with {user} ({result.Data.Status})");
                        throw new Exception("Could not create tunnel connection");
                    }

                    _tunnelId = result.Data.Id;
                    _log.Information($"Connected to {user}");
                    break;
                }

                case "tunnel/send":
                {
                    var result = JsonConvert.DeserializeObject<DataResponse<TunnelSendResponse>>(json);
                    _groundPlaneId = result.Data.Data.Data.Children.First(x => x.Name == "GroundPlane").Uuid;
                    //File.WriteAllText(@"C:\Users\Richa\Documents\Repositories\Guus Chess\2.1-Remote-Healthcare\Remote Healthcare\Json\Response.json", json);
                    _log.Critical("Groundplane Id = " + _groundPlaneId);
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
        catch (Exception ex)
        {
            _log.Error(ex, "Error while processing incoming message");
            _log.Debug($"Message JSON: {json}");
        }
    }

    private async Task CreateConnectionAsync()
    {
        await _socket.ConnectAsync("145.48.6.10", 6666);
    }
}