

using RemoteHealthcare.Client.Data;
using RemoteHealthcare.Client.Data.Providers.Bike;
using RemoteHealthcare.Client.Data.Providers.Heart;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.Client
{
    public class VrConnection
    {
        BikeDataProvider bike;
        HeartDataProvider heart;
        EngineConnection engine;

        public VrConnection(BikeDataProvider bike, HeartDataProvider heart, EngineConnection engine)
        {
            this.bike = bike;
            this.heart = heart;
            this.engine = engine;

            

            /* Unmerged change from project 'Client (net6.0)'
            Before:
                    }

                    public async void start()
            After:
                    }

                    public async void start()
            */
        }

        public async void Start()
        {
            while (true)
            {
                await bike.ProcessRawData();
                ///uncommend
                //await engine.ChangeBikeSpeed(bike.GetData().Speed); 
                Thread.Sleep(300);

                byte[] data = (new byte[] {164, 9, 78, 5, 48, 255, 255, 255, 255, 255, 255, 199, 0 });
                byte checksum = data[0];
                for (int i = 1; i < 12; i++)
                {
                    checksum ^= data[i];
                }
                data[12] = (byte)checksum;
                
                Console.WriteLine(BitConverter.ToString(data));
                await bike.SendMessage(data);
            }
        }
        public BikeData getBikeData()
        {
            return bike.GetData();
        }

        internal HeartData getHearthData()
        {
            return heart.GetData();
        }
    }
}