using System.Security.Cryptography;
using System.Text;

namespace RemoteHealthcare.Common.Socket;

public static class SocketHelper
{
    private static readonly RSACryptoServiceProvider Rsa = new(2048);

    public static async Task SendMessage(Stream stream, string data, bool useEncryption = true)
    {
        var bytes = Encode(data, useEncryption);
        
        await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
        await stream.WriteAsync(bytes, 0, data.Length);
    }

    public static async Task<string> ReadMessage(Stream stream, bool useEncryption = true)
    {
        var length = new byte[4];
        await stream.ReadAsync(length, 0, 4);
        var dataLength = BitConverter.ToInt32(length, 0);

        var dataRead = 0;
        var data = new byte[dataLength];
        
        while (dataRead < dataLength)
        {
            var read = await stream.ReadAsync(data, dataRead, dataLength - dataRead);
            dataRead += read;
        }

        var message = new List<byte>();
        message.AddRange(length);
        message.AddRange(data);

        return Decode(message.ToArray(), useEncryption);
    }

    public static byte[] Encode(string text, bool useEncryption = true)
    {
        // Convert the text to bytes
        var textBytes = Encoding.Unicode.GetBytes(text);
        
        // Encrypt the bytes using RSA
        if(useEncryption)
            textBytes = Rsa.Encrypt(textBytes, false);;

        // If the bytes were null, throw an exception
        if(textBytes == null)
            throw new NullReferenceException("Bytes is null");

        var length = textBytes.Length;
        var lengthBytes = BitConverter.GetBytes(length);
        
        if(lengthBytes.Length > 4)
            throw new Exception($"Length of encrypted bytes is too large (length: {length})");

        var bytes = new List<byte>();
        bytes.AddRange(lengthBytes);
        bytes.AddRange(textBytes);

        return bytes.ToArray();
    }

    public static string Decode(byte[] data, bool useEncryption = true)
    {
        var textBytes = data.Skip(4).ToArray();
        
        // Decrypt the bytes using RSA
        if(useEncryption)
            textBytes = Rsa.Decrypt(textBytes, false);
        
        // If the bytes were null, throw an exception
        if(textBytes == null)
            throw new NullReferenceException("Bytes is null");
        
        // Convert the bytes to a string
        var text = Encoding.Unicode.GetString(textBytes);

        return text;
    }

    public static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
}