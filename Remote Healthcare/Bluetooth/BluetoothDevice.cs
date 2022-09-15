using System.Runtime.InteropServices;
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
    private readonly Predicate<byte[]>? _predicate;

    public byte[] ReceivedData { get; private set; } = new byte[12];
    public string ServiceName { get; private set; } = string.Empty;
    
    public BluetoothDevice(string deviceName, string serviceName,string serviceCharacteristic, Predicate<byte[]>? predicate = null)
    {
        _deviceName = deviceName;
        _serviceName = serviceName;
        _predicate = predicate;
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
            Thread.Sleep(1000);
            Console.WriteLine($"{_deviceName} connnecting.{errorCode}");
            errorCode = await _bluetoothConnection.OpenDevice(_deviceName);
            Console.WriteLine($"{_deviceName} connnecting..{errorCode}");
            var services = _bluetoothConnection.GetServices;
            errorCode = await _bluetoothConnection.SetService(_serviceName);
            Console.WriteLine($"{_deviceName} connnecting... {errorCode}");
            //_bluetoothConnection.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            Console.WriteLine($"{_deviceName} connnecting.... {errorCode}");
            _bluetoothConnection.SubscriptionValueChanged += (sender, e) =>
            {
                if (_predicate != null && !_predicate.Invoke(e.Data)) return;

                ServiceName = e.ServiceName;
                ReceivedData = e.Data;
            };

            errorCode = await _bluetoothConnection.SubscribeToCharacteristic(_serviceCharacteristic);
            Console.WriteLine($"{_deviceName} connected");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Could not connect to bluetooth device {_deviceName} (Error code: {errorCode})");
            throw;
        }
    }
    private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
    {
        //Console.WriteLine(BitConverter.ToString(e.Data).Replace("-", " "));
        if (e.Data.Length < 5)
        {
            //ReceivedData = e.Data;
        }
        else if (e.Data[4] == 16)
        {
            
            //ReceivedData = e.Data;


            /*String ID = "Bluetooth";
            String Speed = (e.Data[8] + (e.Data[9] * 255)) * 0.0036f;
            String Distance = e.Data[7];
            String HearthRate = e.Data[10];
            String TimeInSeconds = e.Data[6] * 0.25;
            String DeviceType = DeviceType.Bike;*/

           /* double speed = (e.Data[8] + (e.Data[9] * 255)) * 0.0036;
            int distance = e.Data[7];
            double time = e.Data[6] * 0.25;


            Console.WriteLine("Speed:{0}, distance:{1}, time:{2}", speed, distance, time);*/
            //Console.WriteLine(BitConverter.ToString(e.Data).Replace("-", " "));

        }


        //console.writeline("received from {0}: {1}, {2}", e.servicename,bitconverter.tostring(e.data).replace("-", " "),encoding.utf8.getstring(e.data));
    }
}