using System.Security.Cryptography;
using System.Text;
using RemoteHealthcare.Common.Cryptography;
using RemoteHealthcare.Common.Logger;
using MemoryStream = System.IO.MemoryStream;

namespace RemoteHealthcare.Common.Socket;

public static class SocketHelper
{
    private static readonly byte[] Cypher = { 0x20, 0x0A, 0xB0, 0x03, 0x01, 0xC2, 0xC2, 0x12, 0x05, 0xBC, 0xCC, 0xA9, 0x9F, 0xFF, 0xCD, 0xD2 };

    public static async Task SendMessage(Stream stream, string data, bool useEncryption = true)
    {
        var bytes = Encode(data, useEncryption);
        
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public static async Task<string> ReadMessage(Stream stream, bool useEncryption = true)
    {
        var length = new byte[4];
        var dataRead = 0;

        while (dataRead < 4)
        {
            var read = await stream.ReadAsync(length, dataRead, 4 - dataRead);
            dataRead += read;
        }
        var dataLength = BitConverter.ToInt32(length, 0);
        
        dataRead = 0;
        var data = new byte[dataLength];
        
        while (dataRead < dataLength)
        {
            var read = await stream.ReadAsync(data, dataRead, dataLength - dataRead);
            dataRead += read;
        }

        var bytes = new List<byte>();
        bytes.AddRange(length);
        bytes.AddRange(data);

        return Decode(bytes.ToArray(), useEncryption);
    }

    public static byte[] Encode(string text, bool useEncryption = true)
    {
        // Convert the text to bytes
        var textBytes = Encoding.UTF8.GetBytes(text);
        
        // Encrypt the bytes
        if (useEncryption)
            textBytes = Encryption.Encrypt(textBytes, Cypher);

        // If the bytes were null, throw an exception
        if(textBytes == null)
            throw new NullReferenceException("Bytes is null");

        var length = textBytes.Length;
        var lengthBytes = BitConverter.GetBytes(length);
        
        if(lengthBytes.Length > 4)
            throw new Exception($"Length of encrypted bytes is too large (length: {length})");

        lengthBytes = lengthBytes.Take(4).ToArray();
        
        var bytes = new List<byte>();
        bytes.AddRange(lengthBytes);
        bytes.AddRange(textBytes);

        return bytes.ToArray();
    }

    public static string Decode(byte[] data, bool useEncryption = true)
    {
        var textBytes = data.Skip(4).ToArray();
        
        // Decrypt the bytes
        if (useEncryption)
            textBytes = Encryption.Decrypt(textBytes, Cypher);
        
        // If the bytes were null, throw an exception
        if(textBytes == null)
            throw new NullReferenceException("Bytes is null");
        
        // Convert the bytes to a string
        var text = Encoding.UTF8.GetString(textBytes);

        return text;
    }
}