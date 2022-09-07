namespace RemoteHealthcare.Data.Providers.Heart;

public class SimulationHeartDataProvider : HeartDataProvider
{
	public override Task Initialise() => Task.CompletedTask;
	
	public override async Task ProcessRawData()
	{
		SetId("Simulation");
		SetHeartRate(new Random().Next(60, 100));
	}
}