using RemoteHealthcare.Bluetooth;

namespace RemoteHealthcare.Data.Providers.Heart;

public class BluetoothHeartDataProvider : HeartDataProvider
{
	private readonly BluetoothDevice _heartSensor = new("Decathlon Dual HR", "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");
	
	public override Task Initialise() => _heartSensor.Connect();

	public override async Task ProcessRawData()
	{
		SetId(_heartSensor.ServiceName);
		SetHeartRate(_heartSensor.ReceivedData[1]);
	}
}