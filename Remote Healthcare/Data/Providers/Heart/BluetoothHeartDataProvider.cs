using System.Threading.Tasks;
using Avans.TI.BLE;

namespace RemoteHealthcare.Data.Providers.Heart;

public class BluetoothHeartDataProvider : HeartDataProvider
{
    static int heartRate = 0;
    static string id = "";
    public override async Task Process()
    {

        int errorCode = 0;
        BLE bleHeart = new BLE();
        Thread.Sleep(1000);
        await Task.CompletedTask;

        var services = bleHeart.GetServices;

        errorCode = await bleHeart.OpenDevice("Decathlon Dual HR");

        await bleHeart.SetService("HeartRate");
       

        bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
        await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");
        SetId(id);
        SetHeartRate(heartRate);
        
    }
    private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
    {
        


        if (e.ServiceName != "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e")
        {

            try
            {
                id = e.ServiceName;
                heartRate = e.Data[1];
                

                

            }
            catch
            {
                heartRate = 0;
                id = "";
            }

        }
        


    }
}