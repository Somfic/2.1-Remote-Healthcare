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
}