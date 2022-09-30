using RemoteHealthcare.Bluetooth;

namespace RemoteHealthcare.Data.Providers.Heart;

public class BluetoothHeartDataProvider : HeartDataProvider
{
    private readonly BluetoothDevice
        _heartSensor = new("Decathlon Dual HR", "HeartRate", "HeartRateMeasurement","", 0, 16);

    public override async Task Initialise()
    {
        await _heartSensor.Connect();
    }

    public override async Task ProcessRawData()
    {
        SetId(_heartSensor.ServiceName);
        SetHeartRate(_heartSensor.ReceivedData[1]);
    }
}