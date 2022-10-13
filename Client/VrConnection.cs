using RemoteHealthcare.Client.Data.Providers.Bike;
using RemoteHealthcare.Client.Data.Providers.Heart;
using RemoteHealthcare.NetworkEngine;

namespace NetworkEngine.Socket
{
    public class VrConnection
    {
        BikeDataProvider bike;
        HeartDataProvider heart;
        public EngineConnection engine;
        private int resistance;

        public VrConnection(BikeDataProvider bike, HeartDataProvider heart, EngineConnection engine)
        {
            this.bike = bike;
            this.heart = heart;
            this.engine = engine;
        }

        public async void Start()
        {
            var count = 0;
            while (true)
            {
                await bike.ProcessRawData();
                await engine.ChangeBikeSpeed(bike.GetData().Speed);
                await engine.SendTextToInformationPannel(
                    (int)bike.GetData().Speed + "",
                    (int)bike.GetData().Distance + "",
                    bike.GetData().TotalElapsed,
                    heart.GetData().HeartRate.ToString(),
                    resistance.ToString());
                
                Thread.Sleep(300);
            }
        }

        public void setResistance(int resistance)
        {
            this.resistance = resistance;
            byte[] data = (new byte[]
                { 164, 9, 78, 5, 48, 255, 255, 255, 255, 255, 255, (byte)((byte)resistance * 2), 0 });
            byte checksum = data[0];
            for (int i = 1; i < 12; i++)
            {
                checksum ^= data[i];
            }

            data[12] = (byte)checksum;

            Console.WriteLine(BitConverter.ToString(data));
            bike.SendMessage(data);
        }
    }
}