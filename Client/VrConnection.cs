using RemoteHealthcare.Client.Data.Providers.Bike;
using RemoteHealthcare.Client.Data.Providers.Heart;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.Client
{
    public class VrConnection
    {
        BikeDataProvider _bike;
        HeartDataProvider _heart;
        EngineConnection _engine;

        public VrConnection(BikeDataProvider bike, HeartDataProvider heart, EngineConnection engine)
        {
            this._bike = bike;
            this._heart = heart;
            this._engine = engine;

            

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
                await _bike.ProcessRawData();
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
                await _bike.SendMessage(data);
            }
        }
    }
}