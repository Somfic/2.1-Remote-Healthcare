using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;

namespace RemoteHealthcare.Socket
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

            /* Unmerged change from project 'Remote Healthcare (net6.0)'
            Before:
                    }

                    public async void start()
            After:
                    }

                    public async void start()
            */
        }

        public async void start()
        {
            while (true)
            {
                await bike.ProcessRawData();
                await engine.ChangeBikeSpeed(bike.GetData().Speed);
                Thread.Sleep(100);
            }
        }
    }
}
