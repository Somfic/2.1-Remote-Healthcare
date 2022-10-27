using System.Threading.Tasks;
using RemoteHealthcare.Common.Data;
using RemoteHealthcare.Common.Data.Providers.Bike;
using RemoteHealthcare.Common.Data.Providers.Heart;
using RemoteHealthcare.GUIs.Patient.ViewModels;
using RemoteHealthcare.NetworkEngine;

namespace RemoteHealthcare.GUIs.Patient
{
    public class VrConnection
    {
        private readonly BikeDataProvider _bike;
        private readonly HeartDataProvider _heart;
        
        public bool Session;
        private int _resistance;

        private PatientHomepageViewModel _pvm;
        public readonly EngineConnection Engine;

        public VrConnection(BikeDataProvider bike, HeartDataProvider heart, EngineConnection engine)
        {
            _bike = bike;
            _heart = heart;
            Engine = engine;
        }
        
        public async Task Start(PatientHomepageViewModel p)
        {
            await _bike.ProcessRawData();

            _pvm = p;
            while (true)
            {
                if (Session)
                {
                    await _heart.ProcessRawData();
                    await _bike.ProcessRawData();
                    await Engine.ChangeBikeSpeed(_bike.GetData().Speed);

                    _pvm.HeartRate = _heart.GetData().HeartRate.ToString();
                    _pvm.Speed = _bike.GetData().Speed.ToString("##.#");
                    _pvm.Distance = _bike.GetData().Distance.ToString("####.#");
                    _pvm.Time = _bike.GetData().TotalElapsed.ToString("hh\\:mm\\:ss");
                }

                await Engine.SendTextToInformationPannel(_bike.GetData().Speed.ToString("##.#"),
                    _bike.GetData().Distance.ToString("####.#"), _bike.GetData().TotalElapsed,
                    _heart.GetData().HeartRate.ToString(), _resistance.ToString());

                await Task.Delay(1000);
            }
        }
        
        public void SetResistance(int resistance)
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

            _bike.SendMessage(data);
        }

        public BikeData GetBikeData()
        {
            return _bike.GetData();
        }

        internal HeartData GetHearthData()
        {
            return _heart.GetData();
        }
    }
}