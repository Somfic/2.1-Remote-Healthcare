using System.Security.Cryptography;
using System.Text;
using RemoteHealthcare.Common.Logger;
using MemoryStream = System.IO.MemoryStream;

namespace RemoteHealthcare.Common.Socket;

public static class SocketHelper
{
    private static readonly Log Log = new(typeof(SocketHelper));
    
    private static readonly byte[] Key = { 0x20, 0x0A, 0xB0, 0x03, 0x01, 0xC2, 0xC2, 0x12, 0x05, 0xBC, 0xCC, 0xA9, 0x9F, 0xFF, 0xCD, 0xD2};
    private static readonly byte[] Iv = { 0x33, 0x02, 0xA0, 0xAF, 0xF3, 0x2C, 0xDD, 0xAA, 0xB8, 0xF9, 0xFF, 0xC0, 0xFD, 0x91, 0x11, 0x69 };

    public static async Task SendMessage(Stream stream, string data, bool useEncryption = true)
    {
        try
        {
            var bytes = Encode(data, useEncryption);

            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to send message");
        }
    }

    public static async Task<string> ReadMessage(Stream stream, bool useEncryption = true)
    {
        try
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
        catch (Exception ex)
        {
            Log.Warning(ex, "Error reading message from stream");
            return string.Empty;
        }
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
        using (var aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = Key;
            aes.IV = Iv;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, encryptor);
            }
        }
    }

    public static byte[] Decrypt(byte[] data)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = Key;
            aes.IV = Iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, decryptor);
            }
        }
    }
    
    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using (var ms = new MemoryStream())
        using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
        {
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return ms.ToArray();
        }
    }
}