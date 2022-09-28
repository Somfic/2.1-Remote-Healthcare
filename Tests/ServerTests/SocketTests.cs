using System.Text;
using RemoteHealthcare.Common.Socket;

namespace CentralServer.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void NoEncryption()
    {
        var text = GenerateRandomString(100);

        var encrypted = SocketHelper.Encode(text, false);
        var decrypted = SocketHelper.Decode(encrypted, false);

        Assert.That(decrypted, Is.EqualTo(text));
    }

    private string GenerateRandomString(int size)
    {
        var builder = new StringBuilder();
        var random = new Random();

        for (int i = 0; i < size; i++)
        {
            builder.Append((char)random.Next(0, 255));
        }

        return builder.ToString();
    }
}