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

    public async Task SendSkyboxTime(string id, int? hour = null, int? minute = null)
    {
        /* Getting the path of the current directory and then adding the path of the testSave folder and the Time.json 
        file to it. */
        string path = System.Environment.CurrentDirectory;
        path = path.Substring(0, path.LastIndexOf("bin")) + "Json" + "\\Time.json";

        /* This is a method that is used to set the time of the skybox. It is used to set the time of the skybox to the
        time that is given by the user. */
        double? newTime = null;

        if (hour == null)
            hour = 0;

        if (hour >= 0 && hour <= 24)
        {
            if (minute > 0 && minute < 60)
                newTime = (double)hour + ((double)minute / 60.00);
            else if (minute <= 0 || minute == null)
                newTime = (double)hour + ((double)1 / 60.00);
            else if (minute >= 60)
                newTime = (double)hour + ((double)59 / 60.00);
        }
        else if (hour > 0)
            newTime = (double)0 + ((double)minute / 60.00);
        else if (hour >= 24)
            newTime = (double)23 + ((double)59.00 / 60.00);

        JObject jObject = JObject.Parse(File.ReadAllText(path));
        jObject["dest"] = id;
        jObject["time"] = newTime;


        /*
        SendData($@"
             {{
	                 ""id"" : ""tunnel/send"",
	                 ""data"" :
	                 {{
		                     ""dest"" : ""{destID}"",
		                     ""data"" : 
		                     {{
			                      ""id"" : ""scene/skybox/settime"",
			                      ""data"" : 
                                 {{
                                     ""time"" : {newTime}
                                 }}
		                     }}
	                 }}
             }}");
             */
        
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