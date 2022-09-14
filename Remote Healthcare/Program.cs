using Newtonsoft.Json;
using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket;

namespace RemoteHealthcare;

public class Program
{
	
	
	
	public static async Task Main(string[] args)
	{
		var engine = new EngineConnection();
		await engine.ConnectAsync();
		
		var heart = await GetHeartDataProvider();
		var bike = await GetBikeDataProvider();

		while (true)
		{
			await heart.ProcessRawData();
			var heartData = heart.GetData();
			var heartJson = JsonConvert.SerializeObject(heartData);

			await bike.ProcessRawData();
			var bikeData = bike.GetData();
			var bikeJson = JsonConvert.SerializeObject(bikeData);

			Log.Information(bikeJson);
			Log.Information(heartJson);

			await Task.Delay(1000);
		}
	}

	private static async Task<HeartDataProvider> GetHeartDataProvider()
	{
		try
		{
			var provider = new BluetoothHeartDataProvider();
			await provider.Initialise();
			return provider;
		}
		catch (PlatformNotSupportedException)
		{
			Log.Debug("Switching to simulated heart provider");
			var provider = new SimulationHeartDataProvider();
			await provider.Initialise();
			return provider;
		}
	}

	private static async Task<BikeDataProvider> GetBikeDataProvider()
	{
		try
		{
			var provider = new BluetoothBikeDataProvider();
			await provider.Initialise();
			return provider;
		}
		catch (PlatformNotSupportedException)
		{
			Log.Debug("Switching to simulated bike provider");
			var provider = new SimulationBikeDataProvider();
			await provider.Initialise();
			return provider;
		}
	}
}