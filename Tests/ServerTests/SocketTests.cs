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
    public void Checksum()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0x06 };

        var sum = bytes.Take(bytes.Length - 1).Sum(x => x);
        var checkByte = BitConverter.GetBytes(sum).First();

        Assert.That(bytes.Last(), Is.EqualTo(checkByte));
    }

    [Test]
    public void Encryption()
    {
        var text = GenerateRandomString(10000);

        var encrypted = SocketHelper.Encode(text, true);
        var decrypted = SocketHelper.Decode(encrypted, true);

        Assert.AreEqual(text, decrypted);
    }

    [Test]
    public void NoEncryption()
    {
        var text = GenerateRandomString(10000);

        var encrypted = SocketHelper.Encode(text, false);
        var decrypted = SocketHelper.Decode(encrypted, false);

        Assert.AreEqual(text, decrypted);
        
    }

    [Test]
    public async Task Reading()
    {
        var text = GenerateRandomString(100);
        var data = SocketHelper.Encode(text);
        
        var stream = new MemoryStream(data);
        var readText = await SocketHelper.ReadMessage(stream);
        
        Assert.AreEqual(text, readText);
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