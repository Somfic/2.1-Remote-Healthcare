using Avans.TI.BLE;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Bluetooth;

public class BluetoothDevice
{
    private BLE _bluetoothConnection;

    private readonly string _deviceName;
    private readonly string _serviceName;

    public byte[] ReceivedData { get; private set; } = new byte[12];
    public string ServiceName { get; private set; } = string.Empty;
    
    public BluetoothDevice(string deviceName, string serviceName)
    {
        _deviceName = deviceName;
        _serviceName = serviceName;
    }

    public async Task Connect()
    {
        try
        {
            _bluetoothConnection = new BLE();

            await _bluetoothConnection.OpenDevice(_deviceName);
            await _bluetoothConnection.SetService(_serviceName);

            _bluetoothConnection.SubscriptionValueChanged += (sender, e) =>
            {
                ServiceName = e.ServiceName;
                ReceivedData = e.Data;
            };

            await _bluetoothConnection.SubscribeToCharacteristic(_serviceName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Could not connect to bluetooth device {_deviceName}");
        }
    }
}