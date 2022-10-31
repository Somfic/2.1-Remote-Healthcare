
using RemoteHealthcare.Common.Bluetooth;

namespace RemoteHealthcare.Common.Data.Providers.Bike;

public class BluetoothBikeDataProvider : BikeDataProvider
{
    private readonly BluetoothDevice _bikeSensor;

    public BluetoothBikeDataProvider(string serial)
    {
        _bikeSensor = new BluetoothDevice($"Tacx Flux {serial}", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e",
            "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e", "6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", 4, 16);
    }

    public override async Task Initialise()
    {
        await _bikeSensor.Connect();
    }

    public override async Task ProcessRawData()
    {
        int checkSum = 0;

        for (int i = 0; i < 12; i++)
        {
            checkSum ^= _bikeSensor.ReceivedData[i];
        }

        if (checkSum % 255 == _bikeSensor.ReceivedData[12])
        {
            SetId(_bikeSensor.ServiceName);
            SetSpeed((_bikeSensor.ReceivedData[8] + _bikeSensor.ReceivedData[9] * 255) * 0.0036f);
            SetDistance(_bikeSensor.ReceivedData[7]);
            SetHeartRate(_bikeSensor.ReceivedData[10]);
            SetElapsed(TimeSpan.FromSeconds(_bikeSensor.ReceivedData[6] * 0.25));
            SetDeviceType(DeviceType.Bike);
        }
    }

    public override async Task SendMessage(byte[] bytes)
    {
        await _bikeSensor.SendMessage(bytes);
    }
}