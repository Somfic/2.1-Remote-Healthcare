using System.Security.Cryptography;
using System.Text;
using RemoteHealthcare.Common.Logger;
using MemoryStream = System.IO.MemoryStream;

namespace RemoteHealthcare.Common.Socket;

public static class SocketHelper
{
    private static readonly RSACryptoServiceProvider Rsa = new(2048);
    private static readonly Aes Aes = Aes.Create();

    private static readonly Log Log = new(typeof(SocketHelper));

    public static async Task SendMessage(Stream stream, string data, bool useEncryption = true)
    {
        Log.Debug($"Sending: '{data}' ({(useEncryption ? "encrypted" : "unencrypted")})");
        
        var bytes = Encode(data, useEncryption);
        
        Log.Debug($"Sending {string.Join(" ", bytes)} ({bytes.Length})");
        
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

        Log.Debug("Incoming message length: " + dataLength);
        
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
        
        Log.Debug($"Receiving {string.Join(" ", bytes)} ({bytes.Count})");

        return Decode(bytes.ToArray(), useEncryption);
    }

    public static byte[] Encode(string text, bool useEncryption = true)
    {
        // Convert the text to bytes
        var textBytes = Encoding.UTF8.GetBytes(text);
        
        // Encrypt the bytes
        if (useEncryption)
            textBytes = Encrypt(textBytes);

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
            textBytes = Decrypt(textBytes);
        
        // If the bytes were null, throw an exception
        if(textBytes == null)
            throw new NullReferenceException("Bytes is null");
        
        // Convert the bytes to a string
        var text = Encoding.UTF8.GetString(textBytes);

        return text;
    }

    public static byte[] Encrypt(byte[] data)
    {
        Aes.Padding = PaddingMode.PKCS7;
        
        var encryptor = Aes.CreateEncryptor(Aes.Key, Aes.IV);
        var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
        
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();

        return ms.ToArray();
    }

    public static byte[] Decrypt(byte[] data)
    {
        Aes.Padding = PaddingMode.PKCS7;
        
        var decryptor = Aes.CreateDecryptor(Aes.Key, Aes.IV);
        var decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);
        
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Write);
        
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        
        return decrypted;
    }
}