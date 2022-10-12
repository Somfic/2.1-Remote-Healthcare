using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Models;
using System.Timers;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class SessionData
{
    public string SessionId { get; set; }
    public string DeviceType { get; set; }
    public string Id { get; set; }

    public List<SessionMiniData> MiniDatas {get; set;}

    public SessionData(string sessionID, string deviceType, string id)
    {
        MiniDatas = new List<SessionMiniData>();

        SessionId = sessionID;
        DeviceType = deviceType;
        Id = id;
        
    }

    public bool addData(JObject data)
    {
        if (!SessionId.Equals(data["sessionId"].ToObject<string>()) || !DeviceType.Equals(data["deviceType"].ToObject<string>()) || !Id.Equals(data["id"].ToObject<string>()))
        {
            return false;
        }
        MiniDatas.Add(new SessionMiniData(data["speed"].ToObject<int>(), data["distance"].ToObject<int>(), data["heartRate"].ToObject<int>(), data["elapsed"].ToObject<int>()));
        return true;
        
    }

    public bool addData(string sessionID, int speed, int distance, int heartrate, int elapsed, string deviceType, string id)
    {
        if (!SessionId.Equals(sessionID) || !DeviceType.Equals(deviceType) || !Id.Equals(id))
        {
            return false;
        }
        MiniDatas.Add(new SessionMiniData(speed, distance, heartrate, elapsed));
        return true;
    }
}