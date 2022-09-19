using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Socket;

public class Socket
{
    private byte[] _totalBuffer = Array.Empty<byte>();
    private readonly byte[] _buffer = new byte[1024];

    private readonly Log _log = new(typeof(Socket));
    private readonly TcpClient _socket = new();
    private NetworkStream _stream;

    public async Task ConnectAsync(string host, int port)
    {
        if (_socket.Connected)
            return;

        try
        {
            var ip = IPAddress.Parse(host);

            _log.Debug($"Connecting to {ip}:{port} ... ");

            await _socket.ConnectAsync(ip, port);

            _stream = _socket.GetStream();
            _stream.BeginRead(_buffer, 0, 1024, OnRead, null);

            _log.Debug($"Connected to {ip}:{port}");
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"Could not connect to {host}:{port}");
            throw;
        }
    }

    private void OnRead(IAsyncResult readResult)
    {
        try
        {
            var numberOfBytes = _stream.EndRead(readResult);
            _totalBuffer = Concat(_totalBuffer, _buffer, numberOfBytes);
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Could not read from stream");
            return;
        }

        while (_totalBuffer.Length >= 4)
        {
            var packetSize = BitConverter.ToInt32(_totalBuffer, 0);

            if (_totalBuffer.Length >= packetSize + 4)
            {
                var json = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);
                OnMessage?.Invoke(this, json);

                var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                _totalBuffer = newBuffer;
            }

            else
                break;
        }

        _stream.BeginRead(_buffer, 0, 1024, OnRead, null);
    }

    public async Task SendAsync(string id, dynamic? data = null)
    {
        var command = new { id = id, data = data };
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task SendTerrain(string dest, dynamic? data = null)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Terrain.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        JArray heights = jObject["data"]["data"]["data"]["heights"] as JArray;
        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                heights.Add(0);
            }
        }

        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task AddNode(string dest, dynamic? data = null)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\SendNodeAdd.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task GetScene(string dest, dynamic? data = null)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\GetScene.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task RemoveGroundPlane(string dest, string groundPlaneID)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\RemoveNode.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["id"] = groundPlaneID;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task AddRoute(string dest)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddRoute.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task AddRoad(string dest, string routeId)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddRoad.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["route"] = routeId;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task SendSkyboxTime(string id, double time)
    {
        /* Getting the path of the current directory and then adding the path of the testSave folder and the Time.json 
        file to it. */
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Time.json";
        
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = id;
        jObject["data"]["data"]["data"]["time"] = time;

        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject));
    }
        
    public async Task SendTerrain(string dest, dynamic? data = null)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Terrain.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        JArray heights = jObject["data"]["data"]["data"]["heights"] as JArray;
        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                heights.Add(0);
            }
        }

        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task AddNode(string dest, dynamic? data = null)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\SendNodeAdd.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;

        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task GetScene(string dest, dynamic? data = null)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\GetScene.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task RemoveGroundPlane(string dest, string groundPlaneID)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\RemoveNode.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["id"] = groundPlaneID;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task AddRoute(string dest)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddRoute.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task AddRoad(string dest, string routeId)
    {
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\AddRoad.json";
        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["data"]["dest"] = dest;
        jObject["data"]["data"]["data"]["route"] = routeId;
        
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject));
        _log.Debug(JsonConvert.SerializeObject(jObject.ToString()));
        await _stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4);
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }
    
    public event EventHandler<string> OnMessage;

    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
}