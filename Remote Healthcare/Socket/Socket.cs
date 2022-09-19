using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Socket;

public class Socket
{
    private readonly byte[] _buffer = new byte[1024];

    private readonly Log _log = new(typeof(Socket));
    private readonly TcpClient _socket = new();
    private NetworkStream _stream;
    private byte[] _totalBuffer = Array.Empty<byte>();

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
            {
                break;
            }
        }

        _stream.BeginRead(_buffer, 0, 1024, OnRead, null);
    }

    public async Task SendAsync(string id, dynamic? data)
    {
        var command = new { id, data };
        var json = JsonConvert.SerializeObject(command);
        await SendAsync(json);
    }

    public async Task SendAsync(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
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