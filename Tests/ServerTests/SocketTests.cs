using RemoteHealthcare.Common.Socket;

namespace RemoteHealthcare.Tests.ServerTests;

public class SocketTests
{
    [Test]
    public void NoEncryption()
    {
        var text = TestHelper.GenerateRandomString();

        var encrypted = SocketHelper.Encode(text, false);
        var decrypted = SocketHelper.Decode(encrypted, false);

        Assert.That(decrypted, Is.EqualTo(text));
    }

    [Test]
    public void ByteEncryption()
    {
        var bytes = TestHelper.GenerateRandomBytes();
        
        var encrypted = SocketHelper.Encrypt(bytes);
        var decrypted = SocketHelper.Decrypt(encrypted);
        
        Assert.Multiple(() =>
        {
            Assert.That(encrypted, Is.Not.EqualTo(bytes));
            Assert.That(decrypted, Is.EqualTo(bytes));
        });
    }

    [Test]
    public void Encryption()
    {
        var text = TestHelper.GenerateRandomString();

        var encrypted = SocketHelper.Encode(text, true);
        var decrypted = SocketHelper.Decode(encrypted, true);

        Assert.That(decrypted, Is.EqualTo(text));
    }
}