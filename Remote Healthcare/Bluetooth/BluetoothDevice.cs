using System.Runtime.InteropServices;
using Avans.TI.BLE;
using RemoteHealthcare.Data;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Bluetooth;

public class BluetoothDevice
{
    private readonly Log _log = new(typeof(BluetoothDevice));
    
    private BLE _bluetoothConnection;

    private readonly string _deviceName;
    private readonly string _serviceName;
    private readonly string _serviceCharacteristic;

    public byte[] ReceivedData { get; private set; } = new byte[12];
    public string ServiceName { get; private set; } = string.Empty;
    
    public BluetoothDevice(string deviceName, string serviceName, string serviceCharacteristic)
    {
        _deviceName = deviceName;
        _serviceName = serviceName;
        _serviceCharacteristic = serviceCharacteristic;
    }

    public async Task Connect()
    {
        var errorCode = -1;
        
        try
        {
            _log.Debug($"Connecting to bluetooth device {_deviceName} ... ");
            
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _log.Warning("Bluetooth is only supported on Windows");
            
            _bluetoothConnection = new BLE();
            await Task.Delay(1000);

            var devices = _bluetoothConnection.ListDevices();
            
            if(!devices.Contains(_deviceName))
            {
                _log.Warning($"Device '{_deviceName}' could not be found");
                throw new ArgumentException("Device could not be found");
            }

            errorCode = await _bluetoothConnection.OpenDevice(_deviceName);
            
            var services = _bluetoothConnection.GetServices;
            errorCode = await _bluetoothConnection.SetService(_serviceName);
            
            _bluetoothConnection.SubscriptionValueChanged += (sender, e) =>
            {
                ServiceName = e.ServiceName;
                ReceivedData = e.Data;
            };

            errorCode = await _bluetoothConnection.SubscribeToCharacteristic(_serviceCharacteristic);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Could not connect to bluetooth device '{_deviceName}', error code: {errorCode}");
            throw;
        }
    }
}