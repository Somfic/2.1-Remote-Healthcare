using System.Security.Cryptography;
using System.Text;

namespace RemoteHealthcare.Common.Cryptography;

public static class Encryption
{
    public static string Encrypt(string data, string cypher)
    {
        var cypherBytes = GenerateCypher(cypher);
        
        var encoded = Encoding.UTF8.GetBytes(data);
        var encrypted = Encrypt(encoded, cypherBytes);
        var decoded = Encoding.UTF8.GetString(encrypted);
        return decoded;
    }
    
    public static byte[] Encrypt(byte[] data, byte[] cypher)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = cypher;
            aes.IV = cypher;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, encryptor);
            }
        }
    }

    public static string Decrypt(string data, string cypher)
    {
        var cypherBytes = GenerateCypher(cypher);
        
        var encoded = Encoding.UTF8.GetBytes(data);
        var encrypted = Decrypt(encoded, cypherBytes);
        var decoded = Encoding.UTF8.GetString(encrypted);
        return decoded;
    }

    public static byte[] Decrypt(byte[] data, byte[] cypher)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = cypher;
            aes.IV = cypher;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, decryptor);
            }
        }
    }
    
    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        return ms.ToArray();
    }
    
    private static byte[] GenerateCypher(string cypher)
    {
        // Generate 16 bytes from the cypher string
        var cypherBytes = new byte[16];
        var cypherBytesLength = cypherBytes.Length;
        var cypherLength = cypher.Length;
        for (var i = 0; i < cypherBytesLength; i++)
        {
            cypherBytes[i] = (byte) cypher[i % cypherLength];
        }

        return cypherBytes;
    }
}