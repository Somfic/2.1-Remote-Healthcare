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
        var bytes = GenerateRandomBytes(1000);

        var encrypted = SocketHelper.Encrypt(bytes);
        var decrypted = SocketHelper.Decrypt(encrypted);

        Assert.That(decrypted, Is.EqualTo(bytes));
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