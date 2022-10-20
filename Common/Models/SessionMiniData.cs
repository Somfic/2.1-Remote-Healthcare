using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.Common.Models
{
    public class SessionMiniData
    {
        public int Distance { get; set; }
        public int Speed { get; set; }
        public int Heartrate { get; set; }
        public int Elapsed { get; set; }

        public SessionMiniData(int speed, int distance, int heartrate, int elapsed)
        {
            Distance = distance;
            Speed = speed;
            Heartrate = heartrate;
            Elapsed = elapsed;
        }

    }
}
