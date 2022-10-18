using RemoteHealthcare.Common.Socket;

namespace RemoteHealthcare.Tests.ServerTests;

public class SocketTests
{
    [Test]
    public void EncodingWithoutEncryption()
    {
        var text = TestHelper.GenerateRandomString(10000);

        var encoded = SocketHelper.Encode(text, false);
        var decoded = SocketHelper.Decode(encoded, false);

        Assert.That(decoded, Is.EqualTo(text));
    }
    
    [Test]
    public void EncodingWithEncryption()
    {
        var text = TestHelper.GenerateRandomString(10000);

        var encoded = SocketHelper.Encode(text);
        var decoded = SocketHelper.Decode(encoded);

        Assert.That(decoded, Is.EqualTo(text));
    }

    [Test]
    public void Encryption()
    {
        var bytes = TestHelper.GenerateRandomBytes(10000);
        
        var encrypted = SocketHelper.Encrypt(bytes);
        var decrypted = SocketHelper.Decrypt(encrypted);

        Assert.That(bytes, Is.Not.EqualTo(encrypted));
        Assert.That(decrypted, Is.EqualTo(bytes));
    }
}