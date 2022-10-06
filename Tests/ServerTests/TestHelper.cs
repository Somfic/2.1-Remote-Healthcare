using System.Text;

namespace RemoteHealthcare.Tests.ServerTests;

public static class TestHelper
{
    public static byte[] GenerateRandomBytes(int size = 1000000)
    {
        var bytes = new byte[size];
        new Random().NextBytes(bytes);
        return bytes;
    }

    public static string GenerateRandomString(int size = 1000000)
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