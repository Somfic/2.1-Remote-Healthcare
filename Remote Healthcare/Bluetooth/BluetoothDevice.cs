﻿using System.Runtime.InteropServices;
using Avans.TI.BLE;
using RemoteHealthcare.Data;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Bluetooth;

public class BluetoothDevice
{
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
            Log.Debug($"Connecting to bluetooth device {_deviceName} ... ");
            
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
               Log.Warning("Bluetooth is only supported on Windows");
            
            _bluetoothConnection = new BLE();
            await Task.Delay(1000);

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
            Log.Error(ex, $"Could not connect to bluetooth device {_deviceName} (Error code: {errorCode})");
            throw;
        }
    }
}