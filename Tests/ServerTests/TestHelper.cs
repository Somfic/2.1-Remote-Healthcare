using System.Text;

namespace RemoteHealthcare.Tests.ServerTests;

public static class TestHelper
{
    public static string GenerateRandomString(int size)
    {
        var builder = new StringBuilder();
        var random = new Random();

        for (var i = 0; i < size; i++)
        {
            builder.Append((char)random.Next(0, 255));
        }

        return builder.ToString();
    }

    public static byte[] GenerateRandomBytes(int size)
    {
        var bytes = new byte[size];
        var random = new Random();

        random.NextBytes(bytes);

        return bytes;
    }
}