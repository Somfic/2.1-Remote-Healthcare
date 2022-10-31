using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Socket.Client;
using RemoteHealthcare.NetworkEngine.Socket.Models;
using RemoteHealthcare.NetworkEngine.Socket.Models.Response;

namespace RemoteHealthcare.NetworkEngine;

public class EngineConnection
{
    private readonly Log _log = new(typeof(EngineConnection));
    private readonly SocketClient _socket = new(false);
    private (string user, string uid)[]? _clients;
    private string _groundPlaneId;
    private string _routeId;
    private string _roadNodeId;
    public bool _isConnected;

    private JArray _hightForHouse;
    private bool[,] _roadArray;
    private bool _roadLoad = false;

    private string _tunnelId;
    private string _userId;
    private string _informationPannelId;
    private string _chatPannelId;
    private string _bikeId;
    private string _terrainNodeId;
    private string _filePath;
    private string _cameraId;
    private string _leftControllerId;
    private string _rightControllerId;
    private string _monkeyHeadId;
    private int _roadcount;

    private int _firstx;
    private int _firstz;
    private bool _first;

    private List<string> _messages = new();

    public EngineConnection()
    {
        _filePath = Environment.CurrentDirectory;
        _filePath = Path.Combine(_filePath.Substring(0, _filePath.LastIndexOf("bin")));
        _socket.OnMessage += async (_, json) => await ProcessMessageAsync(json);
    }

    public async Task<string[]> FindAvailableUsersAsync()
    {
        _clients = null;

        await CreateConnectionAsync();
        await _socket.SendAsync(new { id = "session/list" });

        while (true)
        {
            if (_clients != null)
                return _clients.Select(x => x.user).ToArray();
            _isConnected = true;
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

        if (string.IsNullOrWhiteSpace(user))
        {
            user = Environment.UserName;
            _log.Debug($"Connecting as {user}");
        }

        if (!_clients.Any(x => x.user.ToLower().Contains(user.ToLower())))
        {
            _log.Error(
                $"User '{user}' could not be found. Available users: {string.Join(", ", _clients.Select(x => x.user))}");
            throw new ArgumentException("User could not be found");
        }

        var foundUser = _clients.First(x => x.user.ToLower().Contains(user.ToLower()));
        _userId = foundUser.uid;
        _log.Debug($"Connecting to {foundUser.user} ({foundUser.uid})");

        await _socket.SendAsync(new { id = "tunnel/create", data = new { session = _userId, key = password } });

        await Task.Delay(1000);
        await ResetScene(_tunnelId);

        await Task.Delay(1000);
        await SendSkyboxTime(_tunnelId, 10.5);

        // await SendTerrain(_tunnelId);
        await Task.Delay(1000);
        await Heightmap(_tunnelId);

        await Task.Delay(1000);
        await CreateTerrainNode(_tunnelId);

        await Task.Delay(1000);
        await AddTerrainLayer(_tunnelId);

        await Task.Delay(1000);
        await GetScene(_tunnelId);

        await Task.Delay(1000);
        await RemoveNode(_groundPlaneId);
        //await RemoveNode(_leftControllerId);
        //await RemoveNode(_rightControllerId);

        await Task.Delay(1000);
        await AddRoute(_tunnelId);

        await Task.Delay(1000);
        await AddRoad(_tunnelId, _routeId);

        await Task.Delay(1000);
        await AddBikeModel(_tunnelId);

        await Task.Delay(1000);
        await PlaceBikeOnRoute(_tunnelId);

        

        await Task.Delay(1000);
        await AddInformationPannelNode(_tunnelId);

        await Task.Delay(1000);
        await AddChatPannelNode(_tunnelId);

        await Task.Delay(1000);
        await MoveCameraPosition();
        await Task.Delay(1000);
        await MoveHeadPosition();
        
        await Task.Delay(1000);
        await ChangeBikeSpeed(50);
        await Task.Delay(1000);
        await RoadLoad();
        
        await Task.Delay(1000);
        await Addhouses(_tunnelId, 100);

       
    }


    private async Task RoadLoad()
    {
        _roadArray = new bool[256, 256];

        string s = Path.Combine(_filePath, "Roadload", "road.ser");
        BinaryFormatter b = new BinaryFormatter();
        if (!File.Exists(s))
        {
            await ChangeBikeSpeed(50);
            while (_roadcount < 550)
            {
                await Task.Delay(50);
                await NodeInfo(_tunnelId);
            }


            Stream ss = new FileStream(s, FileMode.Create, FileAccess.Write);
            b.Serialize(ss, _roadArray);
        }
        else
        {
            Stream ss = new FileStream(s, FileMode.Open, FileAccess.Read);
            _roadArray = (bool[,])b.Deserialize(ss);
        }
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
                    _clients = result.Data.OrderByDescending(x => x.LastPing).Select(x =>
                        (user: $"{x.Client.Host}/{x.Client.User} ({Math.Round((DateTime.Now - x.LastPing).TotalSeconds)}s)",
                            uid: x.Id)).ToArray();
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
                    _log.Debug($"Connected to {user}");
                    break;
                }

                case "tunnel/send":
                {
                    string resultSerial = "";
                    var result = new DataResponse<TunnelSendResponse>();
                    // var result = JsonConvert.DeserializeObject<DataResponse<TunnelSendResponse>>(json);
                    // string resultSerial = result.Data.Data.Serial;
                    try
                    {
                        result = JsonConvert.DeserializeObject<DataResponse<TunnelSendResponse>>(json);
                        resultSerial = result.Data.Data.Serial;
                    }
                    catch
                    {
                        resultSerial = raw.data.data.serial;
                    }

                    switch (resultSerial)
                    {
                        case "1":
                        {
                            _groundPlaneId = result.Data.Data.Data.Children.First(x => x.Name == "GroundPlane").Uuid;
                            _cameraId = result.Data.Data.Data.Children.First(x => x.Name == "Camera").Uuid;
                            _leftControllerId = result.Data.Data.Data.Children.First(x => x.Name == "LeftHand").Uuid;
                            _rightControllerId = result.Data.Data.Data.Children.First(x => x.Name == "RightHand").Uuid;
                            _monkeyHeadId = result.Data.Data.Data.Children.First(x => x.Name == "Head").Uuid;
                            _log.Debug("Head Id = " + _monkeyHeadId);
                            break;
                        }

                        case "2":
                        {
                            _bikeId = result.Data.Data.Data.Uuid;
                            _log.Debug(JObject.Parse(json).ToString());
                            break;
                        }

                        case "3":
                        {
                            _routeId = result.Data.Data.Data.Uuid;
                            _log.Debug("Route ID is: " + _routeId);
                            _log.Debug(JObject.Parse(json).ToString());
                            break;
                        }

                        case "4":
                        {
                            _roadNodeId = result.Data.Data.Data.Uuid;
                            _log.Debug("Road Node ID is: " + _roadNodeId);
                            break;
                        }

                        case "5":
                        {
                            _terrainNodeId = result.Data.Data.Data.Uuid;
                            _log.Debug("Terrain Node ID is: " + _terrainNodeId);
                            break;
                        }
                        case "10":
                        {
                            _informationPannelId = result.Data.Data.Data.Uuid;
                            _log.Debug("Pannel Node ID is: " + _informationPannelId);
                            break;
                        }
                        case "9":
                        {
                            GetBikePos(raw.data.data.data[0].components[0].position[0].ToString(),
                                raw.data.data.data[0].components[0].position[2].ToString());
                            break;
                        }
                        case "11":
                        {
                            _chatPannelId = result.Data.Data.Data.Uuid;
                            _log.Debug("Pannel Node ID is: " + _chatPannelId);
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
            _log.Information($"Message JSON: {json}");
        }
    }

    private async Task CreateConnectionAsync()
    {
        await _socket.ConnectAsync("145.48.6.10", 6666);
    }

    // COMMANDS

    public async Task NodeInfo(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "NodeInfo.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;


        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task CreateTerrainNode(string dest, dynamic? data = null)
    {
        string path = Path.Combine(_filePath, "Json", "CreateTerrainNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task GetScene(string dest, dynamic? data = null)
    {
        string path = Path.Combine(_filePath, "Json", "GetScene.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task ChangeBikeSpeed(double speed)

    {
        string path = Path.Combine(_filePath, "Json", "ChangeBikeSpeed.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["node"] = _bikeId;
        jObject["data"]["data"]["data"]["speed"] = speed;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);

        path = Path.Combine(_filePath, "Json", "ChangeAnimationSpeed.json");
        jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = _bikeId;
        jObject["data"]["data"]["data"]["animation"]["speed"] = speed / 10;

        json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }


    public async Task RemoveNode(string nodeId)
    {
        string path = Path.Combine(_filePath, "Json", "RemoveNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = nodeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SendSkyboxTime(string id, double time)
    {
        /* Getting the path of the current directory and then adding the path of the testSave folder and the Time.json 
        file to it. */
        string path = Path.Combine(_filePath, "Json", "Time.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = id;
        jObject["data"]["data"]["data"]["time"] = time;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SendTerrain(string dest, dynamic? data = null)
    {
        string path = Path.Combine(_filePath, "Json", "Terrain.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        var heights = jObject["data"]["data"]["data"]["heights"] as JArray;

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

        _hightForHouse = heights;
        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task Heightmap(string dest)
    {
        string path = Path.Combine(_filePath, "Images", "Heightmap.png");

        using (Bitmap heightmap = new Bitmap(Image.FromFile(path)))
        {
            double[,] heights = new double[heightmap.Width, heightmap.Height];
            for (int x = 0; x < heightmap.Width; x++)
            for (int y = 0; y < heightmap.Height; y++)
                heights[x, y] = ((heightmap.GetPixel(x, y).R / 256.0f) * 25.0f - 5);

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
        string path = Path.Combine(_filePath, "Json", "AddRoad.json");
        var jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["route"] = routeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddBikeModel(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "CreateBikeNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["parent"] = _roadNodeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task PlaceBikeOnRoute(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "FollowRoute.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["route"] = _routeId;
        jObject["data"]["data"]["data"]["node"] = _bikeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task ResetScene(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "ResetScene.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = dest;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddTerrainLayer(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "AddTerrainLayer.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["id"] = _terrainNodeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddInformationPannelNode(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "CreateInformationPannelNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["parent"] = _bikeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddChatPannelNode(string dest)
    {
        string path = Path.Combine(_filePath, "Json", "CreateChatPannelNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["parent"] = _bikeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task GetBikePos(string inputx, string inputz)
    {
        string x = inputx;
        string z = inputz;
        int x1 = (int)Convert.ToDecimal(x);
        int z1 = (int)Convert.ToDecimal(z);
        _roadcount++;
        
        if (!(_firstx == x1 && _firstz == z1))

        {
            for (int i = x1 - 10; i < x1 + 10; i++)
            {
                for (int j = z1 - 10; j < z1 + 10; j++)
                {
                    if (j > -128 && j < 128 && i > -128 && i < 128)
                        _roadArray[i + 128, j + 128] = true;
                }
            }
        }
        else
        {
            _roadLoad = true;
        }

        if (!_first)
        {
            _firstx = x1;
            _firstz = z1;
            _first = true;
        }
    }


    public async Task Addhouses(string dest, int amount)
    {
        Random r = new Random();

        for (int i = 0; i < amount; i++)
        {
            string path = Path.Combine(_filePath, "Json", "AddHouses.json");
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


            int x = r.Next(-128, 128);
            int z = r.Next(-128, 128);
            int y = (int)_hightForHouse[(z + 128) * 256 + (x + 128)];
            // int y = 0;

            if (!_roadArray[x + 128, z + 128])
            {
                var postpar = jObject["data"]["data"]["data"]["components"]["transform"]["position"] as JArray;
                jObject["data"]["dest"] = dest;

                postpar.Insert(0, x);
                postpar.Insert(1, y);
                postpar.Insert(2, z);


                var json = JsonConvert.SerializeObject(jObject);

                await _socket.SendAsync(json);
            }
            else
            {
                continue;
            }
        }
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

    public async Task MoveHeadPosition()
    {
        string path = Path.Combine(_filePath, "Json", "UpdateHeadNode.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = _monkeyHeadId;
        jObject["data"]["data"]["data"]["parent"] = _bikeId;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SendTextToInformationPannel(string speed, string distance, TimeSpan timespan, string bpm,
        string resistance)
    {
        var text =
            $"Snelheid: {speed} \\nAfstand: {distance} \\nTijd: {timespan.Minutes + ":" + timespan.Seconds} \\nHartslag: {bpm} \\nWeerstand: {resistance}";
        await SetBackgroundColor(1, 1, 1, 0.2f, _informationPannelId);
        await ClearPannel(_informationPannelId);
        await AddTextToPannel(text, _informationPannelId);
        await SwapPannel(_informationPannelId);
    }

    public async Task SendTextToChatPannel(string message)
    {
        _messages.Insert(0, message);
        string displayMessage = displayMessages();
        await SetBackgroundColor(1, 1, 1, 0.15f, _chatPannelId);
        await ClearPannel(_chatPannelId);
        await AddTextToPannel(displayMessage, _chatPannelId, 70);
        await SwapPannel(_chatPannelId);
    }

    private string displayMessages()
    {
        string result = "";
        
        for (int i = 0; i < 5; i++)
        {
            int index = 4 - i;
            if (index >= _messages.Count)
            {
                result += "\\n";
                continue;
            }
            
            result += $"{Regex.Replace(_messages[index], ".{30}", "$0\\n")}\\n\\n";
        }

        return result;
    }

    public async Task ClearPannel(string id)
    {
        string path = Path.Combine(_filePath, "Json", "ClearPannel.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = id;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SetBackgroundColor(float r, float g, float b, float t, string id)
    {
        string path = Path.Combine(_filePath, "Json", "ChangeBackgroundColor.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        var postpar = jObject["data"]["data"]["data"]["color"] as JArray;


        postpar.Insert(0, r);
        postpar.Insert(1, g);
        postpar.Insert(2, b);
        postpar.Insert(3, t);


        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = id;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task AddTextToPannel(string text, string id, int? size = 100)
    {
        string path = Path.Combine(_filePath, "Json", "DrawTextOnPannel.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = id;
        jObject["data"]["data"]["data"]["text"] = text;
        jObject["data"]["data"]["data"]["size"] = size;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }

    public async Task SwapPannel(string id)
    {
        string path = Path.Combine(_filePath, "Json", "SwapPannel.json");
        var jObject = JObject.Parse(File.ReadAllText(path));

        jObject["data"]["dest"] = _tunnelId;
        jObject["data"]["data"]["data"]["id"] = id;

        var json = JsonConvert.SerializeObject(jObject);
        await _socket.SendAsync(json);
    }
}