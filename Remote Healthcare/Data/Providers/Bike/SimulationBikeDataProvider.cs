namespace RemoteHealthcare.Data.Providers.Bike;

public class SimulationBikeDataProvider : BikeDataProvider
{
    private readonly Random _random = new();

    public override async Task Process()
    {
        SetId("Simulation");
        SetSpeed((float)(_random.NextDouble() * 30f));
        SetDistance(_random.Next(0, 100));
        SetHeartRate(_random.Next(0, 200));
        SetElapsed(TimeSpan.FromSeconds(_random.Next(64)));
        SetDeviceType(DeviceType.Bike);
    }
}