using Avans.TI.BLE;

namespace RemoteHealthcare.Data.Providers.Bike;

public class BluetoothBikeDataProvider : BikeDataProvider
{
    static BLE BLEBike;
    String id;
    public BluetoothBikeDataProvider(String id)
    {
        this.id = id;
    }

    public async Task Connect()
    {
        int errorCode = 0;
        try
        {
            BLEBike = new BLE();
        } catch
        {
            throw new NotImplementedException("Windows not found");
        }
        Thread.Sleep(1000);
        List<String> bleBikeList = BLEBike.ListDevices();
        Console.WriteLine("Devices found: ");
        foreach (var name in bleBikeList)
        {
            Console.WriteLine($"Device: {name}");
        }
        errorCode = await BLEBike.OpenDevice(id);
        var services = BLEBike.GetServices;
        foreach (var service in services)
        {
            Console.WriteLine($"Service: {service}");
        }
        errorCode = await BLEBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
        BLEBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
        errorCode = await BLEBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

        Console.Read();
    }


    static String ID;
    static float Speed;
    static float Distance;
    static int HearthRate;
    static double TimeInSeconds;
    static DeviceType DeviceType;
    private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
    {
        if (e.Data[4] == 16)
        {
            ID = "Bluetooth";
            Speed = (e.Data[8] + (e.Data[9] * 255)) * 0.0036f;
            Distance = e.Data[7];
            HearthRate = e.Data[10];
            TimeInSeconds = e.Data[6] * 0.25;
            DeviceType = DeviceType.Bike;

            double speed = (e.Data[8] + (e.Data[9] * 255)) * 0.0036;
            int distance = e.Data[7];
            double time = e.Data[6] * 0.25;


            Console.WriteLine("Speed:{0}, distance:{1}, time:{2}", speed, distance, time);
            //Console.WriteLine(BitConverter.ToString(e.Data).Replace("-", " "));

        }


        //console.writeline("received from {0}: {1}, {2}", e.servicename,bitconverter.tostring(e.data).replace("-", " "),encoding.utf8.getstring(e.data));
    }
    public override async Task Process()
    {
        await Task.CompletedTask;
        SetId(ID);
        SetSpeed(Speed);
        SetDistance(Distance);
        SetHeartRate(HearthRate);
        SetElapsed(TimeSpan.FromSeconds(TimeInSeconds));
        SetDeviceType(DeviceType);
    }
}