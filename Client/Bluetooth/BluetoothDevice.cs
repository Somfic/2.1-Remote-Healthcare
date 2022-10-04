using Avans.TI.BLE;
using RemoteHealthcare.Common.Logger;
using System.Runtime.InteropServices;

namespace RemoteHealthcare.Bluetooth;

public class BluetoothDevice
{
    private readonly string _deviceName;
    private readonly Log _log = new(typeof(BluetoothDevice));
    private readonly string _serviceCharacteristic;
    private readonly string _serviceName;
    private BLE _bluetoothConnection;
    private int _idByte;
    private int _id;

    public BluetoothDevice(string deviceName, string serviceName, string serviceCharacteristic, int idByte, int id)
    {
        _deviceName = deviceName;
        _serviceName = serviceName;
        _serviceCharacteristic = serviceCharacteristic;
        _idByte = idByte;
        _id = id;
    }

    public byte[] ReceivedData { get; private set; } = new byte[12];
    public string ServiceName { get; private set; } = string.Empty;

    public async Task SendMessage(byte[] bytes)
    {
        await _bluetoothConnection.WriteCharacteristic(_serviceCharacteristic, bytes);
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

            foreach (var device in devices)
            {
                _log.Information(device);
            }


            if (!devices.Contains(_deviceName))
            {
                throw new Exception("Could not find bluetooth device in list of available connections");
            }

            errorCode = await _bluetoothConnection.OpenDevice(_deviceName);

            var services = _bluetoothConnection.GetServices;
            errorCode = await _bluetoothConnection.SetService(_serviceName);

            _bluetoothConnection.SubscriptionValueChanged += (sender, e) =>
            {
                //Console.WriteLine($" { e.Data[_idByte]}  -- {_id}");

                foreach (byte bite in e.Data)
                {
                    Console.Write(bite + "-");
                }
                Console.WriteLine("");
                if (e.Data[_idByte] == _id)
                {
                  
                    
                    ServiceName = e.ServiceName;
                    ReceivedData = e.Data;
                }
            };

            errorCode = await _bluetoothConnection.SubscribeToCharacteristic(_serviceCharacteristic);
            _log.Information($"Connected to: {_deviceName}");
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"Could not connect to bluetooth device {_deviceName} (Error code: {errorCode})");
            throw;
        }
    }
}