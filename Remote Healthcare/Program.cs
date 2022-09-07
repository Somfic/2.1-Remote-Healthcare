using System.Runtime.InteropServices;
using Newtonsoft.Json;
using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;
using RemoteHealthcare.Logger;

HeartDataProvider heart = new BluetoothHeartDataProvider();
BikeDataProvider bike = new BluetoothBikeDataProvider();

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
	heart = new SimulationHeartDataProvider();
	bike = new SimulationBikeDataProvider();
}

await heart.Initialise();
await bike.Initialise();

while (true)
{
	await heart.ProcessRawData();
	var heartData = heart.GetData();
	var heartJson = JsonConvert.SerializeObject(heartData);

	await bike.ProcessRawData();
	var bikeData = bike.GetData();
	var bikeJson = JsonConvert.SerializeObject(bikeData);

	Log.Information(bikeJson);
	
	await Task.Delay(1000);
}