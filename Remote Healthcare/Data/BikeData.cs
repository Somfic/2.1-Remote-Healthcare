namespace RemoteHealthcare.Data;

public class BikeData : IData
{
    public float Distance { get; set; }

    public float Speed { get; set; }

    public int HeartRate { get; set; }

    public TimeSpan Elapsed { get; set; }

    public DeviceType DeviceType { get; set; }

    public string Id { get; set; } = "";
}

public enum DeviceType
{
    Bike,
    ThreadMill,
    Elliptical,
    RowingMachine,
    StairClimber
}