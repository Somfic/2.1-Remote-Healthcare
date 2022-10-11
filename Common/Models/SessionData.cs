using Newtonsoft.Json.Linq;
using System.Timers;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class SessionData
{
    public string SessionId { get; set; }
    public int Distance { get; set; }
    public int Speed { get; set; }
    public int Heartrate { get; set; }
    public int Elapsed { get; set; }
    public string DeviceType { get; set; }
    public string Id { get; set; }

    public SessionData(JObject data)
    {
        //SessionId = data["sessionId"].ToObject<string>();
        Distance = data["distance"].ToObject<int>();
        Speed = data["speed"].ToObject<int>();
        Heartrate = data["heartRate"].ToObject<int>();
        Elapsed = data["elapsed"].ToObject<int>();
        DeviceType = data["deviceType"].ToObject<string>();
        Id = data["id"].ToObject<string>();
    }
    public SessionData(int speed, int distance, int heartrate, int elapsed, string deviceType, string id)
    {
        //SessionId = data["sessionId"].ToObject<string>();
        Distance = distance;
        Speed = speed;
        Heartrate = heartrate;
        Elapsed = elapsed;
        DeviceType = deviceType;
        Id = id;
    }
}