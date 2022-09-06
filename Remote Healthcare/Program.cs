using Newtonsoft.Json;
using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;

internal class Program {

    static BikeDataProvider bike;
    static HeartDataProvider heart = new BluetoothHeartDataProvider();
    
    private static async Task Main(string[] args)
    {
        BluetoothBikeDataProvider btbike = new BluetoothBikeDataProvider("Tacx Flux 00438");
        
        try
        {
            await btbike.Connect();
            bike = btbike;
        } catch (Exception)
        {
            Console.WriteLine("Switching to simulation");
            Thread.Sleep(5000);
            bike = new SimulationBikeDataProvider();
        }
        HeartDataProvider heart = new BluetoothHeartDataProvider();
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