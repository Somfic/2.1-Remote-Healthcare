using System.Runtime.InteropServices;
using Avans.TI.BLE;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Bluetooth;

public class BluetoothDevice
{
    private BLE _bluetoothConnection;

    private readonly string _deviceName;
    private readonly string _serviceName;
    private readonly Predicate<byte[]>? _predicate;

    public byte[] ReceivedData { get; private set; } = new byte[12];
    public string ServiceName { get; private set; } = string.Empty;
    
    public BluetoothDevice(string deviceName, string serviceName, Predicate<byte[]>? predicate = null)
    {
        _deviceName = deviceName;
        _serviceName = serviceName;
        _predicate = predicate;
    }

    public async Task Connect()
    {
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
               Log.Warning("Bluetooth is only supported on Windows");
            
            _bluetoothConnection = new BLE();

            await _bluetoothConnection.OpenDevice(_deviceName);
            await _bluetoothConnection.SetService(_serviceName);

            _bluetoothConnection.SubscriptionValueChanged += (sender, e) =>
            {
                if (_predicate != null && !_predicate.Invoke(e.Data)) return;
                
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