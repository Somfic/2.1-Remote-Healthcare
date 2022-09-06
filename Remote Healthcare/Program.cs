using Newtonsoft.Json;
using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;

var heart = new BluetoothHeartDataProvider();
var bike = new BluetoothBikeDataProvider();

while (true)
{
	await heart.ProcessRawData();
	var heartData = heart.GetData();
	var heartJson = JsonConvert.SerializeObject(heartData);

	await bike.ProcessRawData();
	var bikeData = bike.GetData();
	var bikeJson = JsonConvert.SerializeObject(bikeData);

	Console.Clear();
	Console.WriteLine($"Heart: {heartJson}");
	Console.WriteLine($" Bike: {bikeJson}");

	await Task.Delay(250);
}