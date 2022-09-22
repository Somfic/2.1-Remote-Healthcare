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
    private JArray hoogte;

    private string _tunnelId;
    private string _userId;
    private string _bikeId;
    private string _terrainNodeId;
    private string _filePath;
    private string _cameraId;



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

        await Task.Delay(1000);
        await MoveCameraPosition();

        await Task.Delay(1000);
        await Addhouses(_tunnelId, 1000);



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
                        _clients = result.Data.OrderByDescending(x => x.LastPing).Select(x => (user: $"{x.Client.Host}/{x.Client.User} ({Math.Round((DateTime.Now - x.LastPing).TotalSeconds)}s)", uid: x.Id)).ToArray();
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
                        case "1":
                        {
                            _groundPlaneId = result.Data.Data.Data.Children.First(x => x.Name == "GroundPlane").Uuid;
                            _cameraId = result.Data.Data.Data.Children.First(x => x.Name == "Camera").Uuid;
                            File.WriteAllText(@"/Users/richardelean/Documents/2.1-Remote-Healthcare/Remote Healthcare/Json/Response.json", JObject.Parse(json).ToString());
                            _log.Critical("Groundplane Id = " + _groundPlaneId);
                            break;
                        }
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
                                case "5":
                        {
                            _terrainNodeId = result.Data.Data.Data.Uuid;
                            _log.Information("Terrain Node ID is: " + _terrainNodeId);
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

        if (data == null)
        {
            for (var i = 0; i < 256; i++)
                for (var j = 0; j < 256; j++)
                    heights.Add(1);
        }
        else
        {
            double[] heightmap = data;
            int x = 0;
            for (var i = 0; i < 256; i++)
            {
                for (var j = 0; j < 256; j++)
                {
                    heights.Add(heightmap[x]);
                    x++;
                }
            }
        }

        _log.Debug(jObject.ToString());
        hoogte = heights;
        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task Heightmap(string dest)
    {
        string path = Path.Combine(_filePath, "Image", "Heightmap.png");

        using (Bitmap heightmap = new Bitmap(Image.FromFile(path)))
        {
            double[,] heights = new double[heightmap.Width, heightmap.Height];
            for (int x = 0; x < heightmap.Width; x++)
                for (int y = 0; y < heightmap.Height; y++)
                    heights[x, y] = (heightmap.GetPixel(x, y).R / 256.0f) * 50.0f - 5;

            SendTerrain(dest, heights.Cast<double>().ToArray());
        }
    }

    public async Task AddRoute(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "AddRoute.json");
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

    public async Task ChangeBikeSpeed(double speed)
    {
        Console.WriteLine("Hallo");
        var path = Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\ChangeBikeSpeed.json";
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["node"] = _bikeId;
        jObject["data"]["data"]["data"]["speed"] = speed;
        Console.WriteLine("Hallo2");

        Console.WriteLine(jObject.ToString());
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

    public async Task MoveCameraPosition()
    {
        string path = Path.Combine(_filePath, "Json", "UpdateCameraNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = _cameraId;
        jObject["data"]["data"]["data"]["parent"] = _bikeId;
        
        var json = JsonConvert.SerializeObject(jObject);
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
    public async Task Addhouses(string dest, int amount)
    {

        Random r = new Random();

        for (int i = 0; i < amount; i++)
        {
            var path = Environment.CurrentDirectory;
            path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddHouses.json";
            var jObject = JObject.Parse(File.ReadAllText(path));
            String s = "";
            switch (r.Next(2))
            {
                case 0:
                    s = $"data/NetworkEngine/models/houses/set1/house{r.Next(1, 27)}.obj";
                    break;
                case 1:
                    s = $"data/NetworkEngine/models/trees/fantasy/tree{r.Next(1, 10)}.obj";
                    break;

            }

            jObject["data"]["data"]["data"]["components"]["model"]["file"] = s;

            int x = r.Next(1, 256);
            int z = r.Next(1, 256);
            int y = (int)hoogte[z * 256 + x];


            var postpar = jObject["data"]["data"]["data"]["components"]["transform"]["position"] as JArray;
            jObject["data"]["dest"] = dest;
            postpar.Insert(0, x);
            postpar.Insert(1, y);
            postpar.Insert(2, z);






            var json = JsonConvert.SerializeObject(jObject);

            await _socket.SendAsync(json);

        }
    }




}