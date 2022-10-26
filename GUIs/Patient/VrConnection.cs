

using System;
using System.Data;
using System.Threading;
using RemoteHealthcare.Client.Data;
using RemoteHealthcare.Client.Data.Providers.Bike;
using RemoteHealthcare.Client.Data.Providers.Heart;
using RemoteHealthcare.GUIs.Patient.ViewModels;
using RemoteHealthcare.NetworkEngine;

namespace NetworkEngine.Socket
{
    public class VrConnection
    {
        BikeDataProvider bike;
        HeartDataProvider heart;
        EngineConnection engine;
        private PatientHomepageViewModel _pvm;

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

        public async void Start(PatientHomepageViewModel p)
        {
            _pvm = p;
            while (true)
            {
                
                await bike.ProcessRawData();
                await heart.ProcessRawData();
                _pvm.Heartrate = heart.GetData().HeartRate.ToString();
                _pvm.Speed = bike.GetData().Speed.ToString();
                _pvm.Distance = bike.GetData().Distance.ToString();
                _pvm.Time = bike.GetData().TotalElapsed.ToString();
                
                Console.WriteLine("Heart: " + heart.GetData().HeartRate);
                // await engine.ChangeBikeSpeed(bike.GetData().Speed);
                Thread.Sleep(300);
            }
        }

        public void setResistance(int resistance)
        {
            byte[] data = (new byte[] { 164, 9, 78, 5, 48, 255, 255, 255, 255, 255, 255, (byte)((byte)resistance * 2), 0 });
            byte checksum = data[0];
            for (int i = 1; i < 12; i++)
            {
                checksum ^= data[i];
            }
            data[12] = (byte)checksum;

            Console.WriteLine(BitConverter.ToString(data));
            bike.SendMessage(data);
        }
        public BikeData getBikeData()
        {
            return bike.GetData();
        }

        public void vrConPRINTTT()
        {
            Console.WriteLine("sudikfhidsfhoikdshfikdsj iufkilsdfhoksd iuhsdifuouihsdf ");
        }
        internal HeartData getHearthData()
        {
           
            return heart.GetData();
        }
    }
}