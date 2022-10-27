using RemoteHealthcare.Common.Bluetooth;

namespace RemoteHealthcare.Common.Data.Providers.Heart;

public class BluetoothHeartDataProvider : HeartDataProvider
{
    private readonly BluetoothDevice
        _heartSensor = new("Decathlon Dual HR", "HeartRate", "HeartRateMeasurement","", 0, 22);

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