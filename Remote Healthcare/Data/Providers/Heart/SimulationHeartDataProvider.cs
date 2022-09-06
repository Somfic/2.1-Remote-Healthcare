namespace RemoteHealthcare.Data.Providers.Heart;

public class SimulationHeartDataProvider : HeartDataProvider
{
    public override async Task Process()
    {
        SetId("Simulation");
        SetHeartRate(new Random().Next(60, 100));
    }
}