using Newtonsoft.Json;
using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;

internal class Program

BikeDataProvider bike = new SimulationBikeDataProvider();
HeartDataProvider heart = new BluetoothHeartDataProvider();

while (true)
{
    private static async Task Main(string[] args)
    {
        BluetoothBikeDataProvider btbike = new BluetoothBikeDataProvider("Tacx Flux 00438");
        BikeDataProvider bike;
        try
        {
            await btbike.Connect();
            bike = btbike;
        } catch (Exception)
        {
            bike = new SimulationBikeDataProvider();
        }
        HeartDataProvider heart = new SimulationHeartDataProvider();
        while (true)
        {
            await heart.Process();
            var heartData = heart.GetData();
            var heartJson = JsonConvert.SerializeObject(heartData);

            await bike.Process();
            var bikeData = bike.GetData();
            var bikeJson = JsonConvert.SerializeObject(bikeData);

            Console.Clear();
            Console.WriteLine($"Heart: {heartJson}");
            Console.WriteLine($" Bike: {bikeJson}");

            await Task.Delay(250);
        }
    }
}