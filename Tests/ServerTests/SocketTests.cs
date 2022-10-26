using System.Text;
using RemoteHealthcare.Common.Socket;

namespace RemoteHealthcare.Tests.ServerTests;

public class SocketTests
{
    [Test]
    public void Encoding()
    {
        var text = GenerateRandomString(100);

        var encoded = SocketHelper.Encode(text, false);
        var decoded = SocketHelper.Decode(encoded, false);

        Assert.That(decoded, Is.EqualTo(text));
    }

    [Test]
    public void Encryption()
    {
        var cypher = GenerateRandomBytes(16);
        var bytes = GenerateRandomBytes(1000);
        
        var encrypted = Common.Cryptography.Encryption.Encrypt(bytes, cypher);
        var decrypted = Common.Cryptography.Encryption.Decrypt(encrypted, cypher);

        Assert.That(bytes, Is.Not.EqualTo(encrypted));
        Assert.That(decrypted, Is.EqualTo(bytes));
    }
    
    [Test]
    public void StringEncryption()
    {
        var cypher = GenerateRandomString(10);
        var text = GenerateRandomString(100);
        
        var encrypted = Common.Cryptography.Encryption.Encrypt(text, cypher);
        var decrypted = Common.Cryptography.Encryption.Decrypt(encrypted, cypher);

        Assert.That(text, Is.Not.EqualTo(encrypted));
        Assert.That(decrypted, Is.EqualTo(text));
    }

    private string GenerateRandomString(int size)
    {
        var builder = new StringBuilder();
        var random = new Random();

        for (var i = 0; i < size; i++)
        {
            builder.Append((char)random.Next(0, 255));
        }

        return builder.ToString();
    }

    private byte[] GenerateRandomBytes(int size)
    {
        var bytes = new byte[size];
        var random = new Random();

        random.NextBytes(bytes);

        return bytes;
    }
}