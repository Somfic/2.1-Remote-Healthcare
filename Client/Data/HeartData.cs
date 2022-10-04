namespace RemoteHealthcare.Client.Data;

public class HeartData : IData
{
    public int HeartRate { get; set; }

    public string Id { get; set; } = "";
}