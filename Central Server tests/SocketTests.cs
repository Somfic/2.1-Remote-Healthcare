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
        var checks = new (byte, byte[])[]
        {

        };
        
        var bytes = new byte[] { 0x33, 0x02, 0x04, 0x07 };
        
        var sum = bytes.Take(bytes.Length - 1).Sum(x => x);
        var checkByte = BitConverter.GetBytes(sum).First();
        
        Assert.That(bytes.Last(), Is.EqualTo(checkByte));
    }
}