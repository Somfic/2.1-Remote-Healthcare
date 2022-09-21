using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;
using RemoteHealthcare.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        
        public async void start()
        {
            while (true)
            {
                bike.ProcessRawData();
                Console.WriteLine(bike.GetData().Speed);
                
                    await engine.ChangeBikeSpeed(bike.GetData().Speed);
                
                Thread.Sleep(500);
            }
        }
    }
}
