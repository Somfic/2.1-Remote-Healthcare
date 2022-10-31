

using System;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using RemoteHealthcare.Common.Data;
using RemoteHealthcare.Common.Data.Providers.Bike;
using RemoteHealthcare.Common.Data.Providers.Heart;
using RemoteHealthcare.GUIs.Patient.ViewModels;
using RemoteHealthcare.NetworkEngine;

namespace NetworkEngine.Socket
{
    public class VrConnection
    {
        BikeDataProvider bike;
        HeartDataProvider heart;

        private PatientHomepageViewModel _pvm;
        public EngineConnection Engine;

        public VrConnection(BikeDataProvider bike, HeartDataProvider heart, EngineConnection engine)
        {
            this.bike = bike;
            this.heart = heart;
            this.Engine = engine;
        }

        public bool session;
        private int _resistance;

        public async Task Start(PatientHomepageViewModel p)
        {
            await bike.ProcessRawData();

            _pvm = p;
            while (true)
            {
                if (session)
                {
                    await heart.ProcessRawData();
                    await bike.ProcessRawData();
                    await Engine.ChangeBikeSpeed(bike.GetData().Speed);

                    _pvm.Heartrate = heart.GetData().HeartRate.ToString();
                    _pvm.Speed = bike.GetData().Speed.ToString("##.#");
                    _pvm.Distance = bike.GetData().Distance.ToString("####.#");
                    _pvm.Time = bike.GetData().TotalElapsed.ToString("hh\\:mm\\:ss");
                    
                }

                await Engine.SendTextToInformationPannel(bike.GetData().Speed.ToString("##.#"), bike.GetData().Distance.ToString("####.#"), bike.GetData().TotalElapsed, heart.GetData().HeartRate.ToString(), _resistance.ToString());

                await Task.Delay(1000);
            }
        }

        
        public void setResistance(int resistance)
            {
                byte[] data = (new byte[]
                    { 164, 9, 78, 5, 48, 255, 255, 255, 255, 255, 255, (byte)((byte)resistance * 2), 0 });
                byte checksum = data[0];
                for (int i = 1; i < 12; i++)
                {
                    checksum ^= data[i];
                }

                data[12] = (byte)checksum;

                _resistance = resistance;
                
                bike.SendMessage(data);
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

    