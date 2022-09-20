using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket.Models;
using RemoteHealthcare.Socket.Models.Response;

namespace RemoteHealthcare.Socket;

public class EngineConnection
{
    private readonly Log _log = new(typeof(EngineConnection));
    private readonly Socket _socket = new();
    private (string user, string uid)[]? _clients;
    private string _groundPlaneId;
    private string _routeId;
    private string _roadNodeId;

    private string _tunnelId;
    private string _userId;
    private string _bikeId;

    public EngineConnection()
    {
        _socket.OnMessage += async (_, json) => await ProcessMessageAsync(json);
    }

    public async Task<string[]> FindAvailableUsersAsync()
    {
        _clients = null;

        await CreateConnectionAsync();
        await _socket.SendAsync("session/list", null);

        while (true)
        {
            if (_clients != null)
                return _clients.Select(x => x.user).ToArray();

            await Task.Delay(50);
        }
    }

    public async Task ConnectAsync(string? user = null, string? password = null)
    {
        await CreateConnectionAsync();
        await FindAvailableUsersAsync();

        if (_clients == null)
        {
            _log.Warning("No clients are available");
            throw new Exception("No clients were found");
        }

        if (user == null)
            user = Environment.UserName;

        if (!_clients.Any(x => x.user.ToLower().Contains(user.ToLower())))
        {
            _log.Warning(
                $"User '{user}' could not be found. Available users: {string.Join(", ", _clients.Select(x => x.user))}");
            throw new ArgumentException("User could not be found");
        }

        var foundUser = _clients.First(x => x.user.ToLower().Contains(user.ToLower()));
        _userId = foundUser.uid;
        _log.Debug($"Connecting to {foundUser.user} ({foundUser.uid}) ... ");

        await _socket.SendAsync("tunnel/create", new { session = _userId, key = password });

        await Task.Delay(1000);
        await ResetScene(_tunnelId);
        await Task.Delay(1000);
        await SendTerrain(_tunnelId);
        await CreateTerrainNode(_tunnelId);

        await Task.Delay(1000);
        await GetScene(_tunnelId);

        await Task.Delay(1000);
        await RemoveGroundPlane(_tunnelId, _groundPlaneId);

        await Task.Delay(1000);
        await AddRoute(_tunnelId);

        await Task.Delay(1000);
        await AddRoad(_tunnelId, _routeId);
        
        await Task.Delay(2000);
        await AddBikeModel(_tunnelId);

        await Task.Delay(1000);
        await PlaceBikeOnRoute(_tunnelId);
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
                    var resultSerial = result.Data.Data.Serial;

                    switch (resultSerial)
                    {
                        case "1":
                        {
                            _groundPlaneId = result.Data.Data.Data.Children.First(x => x.Name == "GroundPlane").Uuid;
                            _log.Critical("Groundplane Id = " + _groundPlaneId);
                            break;
                        }

                        case "2":
                        {
                            _bikeId = result.Data.Data.Data.Uuid;
                            _log.Critical("Bike Id = " + _bikeId);
                            _log.Information(JObject.Parse(json).ToString());
                            break;
                        }
                        
                        case "3":
                        {
                            _routeId = result.Data.Data.Data.Uuid;
                            _log.Information("Route ID is: " + _routeId);
                            _log.Information(JObject.Parse(json).ToString());
                            break;
                        }

                        case "4":
                        {
                            _roadNodeId = result.Data.Data.Data.Uuid;
                            _log.Information("Road Node ID is: " + _roadNodeId);
                            break;
                        }
                        
                        default:
                        {
                            _log.Information(JObject.Parse(json).ToString());
                            break;
                        }
                    }
                    break;
                }

                default:
                {
                    _log.Warning($"Unhandled incoming message with id '{id}'");
                    _log.Debug(json);
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

    // COMMANDS

    public async Task CreateTerrainNode(string dest, dynamic? data = null)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\CreateTerrainNode.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task GetScene(string dest, dynamic? data = null)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\GetScene.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task RemoveGroundPlane(string dest, string groundPlaneID)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\RemoveNode.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["id"] = groundPlaneID;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SendSkyboxTime(string id, double time)
    {
        /* Getting the path of the current directory and then adding the path of the testSave folder and the Time.json 
        file to it. */
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Time.json";

        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = id;
        jObject["data"]["data"]["data"]["time"] = time;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SendTerrain(string dest, dynamic? data = null)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Terrain.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        var heights = jObject["data"]["data"]["data"]["heights"] as JArray;
        for (var i = 0; i < 256; i++)
        for (var j = 0; j < 256; j++)
            heights.Add(0);

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddRoute(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddRoute.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddRoad(string dest, string routeId)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddRoad.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["route"] = routeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddBikeModel(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\CreateBikeNode.json";
        var jObject = JObject.Parse(File.ReadAllText(path));

        var modelPath = Environment.CurrentDirectory;
        modelPath = modelPath.Substring(0, modelPath.LastIndexOf("bin")) + "3DModels" + "\\bike_anim.fbx";
        
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["parent"] = _roadNodeId;
        jObject["data"]["data"]["data"]["components"]["model"]["file"] = modelPath;
        _log.Debug(modelPath);

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task PlaceBikeOnRoute(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\FollowRoute.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["route"] = _routeId;
        jObject["data"]["data"]["data"]["node"] = _bikeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task UpdateBikeNode(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\UpdateNode.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["id"] = _bikeId;
        jObject["data"]["data"]["data"]["parent"] = _roadNodeId;
        
        _log.Debug(jObject.ToString());

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);

    }

    public async Task PauseEngine(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Pause.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        _log.Debug(jObject.ToString());
        await _socket.SendAsync(json);
    }

    public async Task PlayEngine(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Play.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task ResetScene(string dest)
    {
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\ResetScene.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }
}