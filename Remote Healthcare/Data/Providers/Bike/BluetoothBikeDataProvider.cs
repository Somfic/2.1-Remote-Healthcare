using RemoteHealthcare.Bluetooth;

namespace RemoteHealthcare.Data.Providers.Bike;

public class BluetoothBikeDataProvider : BikeDataProvider
{
    private readonly BluetoothDevice _bikeSensor;

    public BluetoothBikeDataProvider(string serial)
    {
        _bikeSensor = new BluetoothDevice($"Tacx Flux {serial}", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e",
            "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");
    }

    public override async Task Initialise()
    {
        await _bikeSensor.Connect();
    }

    public override async Task ProcessRawData()
    {
        SetId(_bikeSensor.ServiceName);
        SetSpeed((_bikeSensor.ReceivedData[8] + _bikeSensor.ReceivedData[9] * 255) * 0.0036f);
        SetDistance(_bikeSensor.ReceivedData[7]);
        SetHeartRate(_bikeSensor.ReceivedData[10]);
        SetElapsed(TimeSpan.FromSeconds(_bikeSensor.ReceivedData[6] * 0.25));
        SetDeviceType(DeviceType.Bike);
    }
}