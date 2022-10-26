﻿

using System;
using System.Threading;
using RemoteHealthcare.Common.Data;
using RemoteHealthcare.Common.Data.Providers.Bike;
using RemoteHealthcare.Common.Data.Providers.Heart;
using RemoteHealthcare.GUIs.Patient.ViewModels;
using RemoteHealthcare.NetworkEngine;

namespace NetworkEngine.Socket
{
    public class VrConnection
    {
        BikeDataProvider _bike;
        HeartDataProvider _heart;
        
        private PatientHomepageViewModel _pvm;
        public EngineConnection Engine;
        
        public VrConnection(BikeDataProvider bike, HeartDataProvider heart, EngineConnection engine)
        {
            this._bike = bike;
            this._heart = heart;
            Engine = engine;
        }

        public bool Session;
        
        public async void Start(PatientHomepageViewModel p)
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
                _pvm.Heartrate = _heart.GetData().HeartRate.ToString();
                _pvm.Speed = _bike.GetData().Speed.ToString("##.#");
                _pvm.Distance = _bike.GetData().Distance.ToString("####.#");
                _pvm.Time = _bike.GetData().TotalElapsed.ToString("hh\\:mm\\:ss");
                Console.WriteLine("Heart: " + _heart.GetData().HeartRate);
                } else {
                    Engine.ChangeBikeSpeed(0);
                }
                Thread.Sleep(300);
            }
        }

        public void SetResistance(int resistance)
        {
            byte[] data = { 164, 9, 78, 5, 48, 255, 255, 255, 255, 255, 255, (byte)((byte)resistance * 2), 0 };
            byte checksum = data[0];
            for (int i = 1; i < 12; i++)
            {
                checksum ^= data[i];
            }
            data[12] = checksum;

            Console.WriteLine(BitConverter.ToString(data));
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