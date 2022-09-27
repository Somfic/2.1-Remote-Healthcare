using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.CentralServer;
[Serializable]
public class SessionData
{
    public string Id { get; set; }
    public Dictionary<string, dynamic> data { get; set; }

    public SessionData(JObject data)
    {
        Id = data["sessionId"].ToObject<string>();
        SetupDictionary(data);
    }

    private void SetupDictionary(JObject data)
    {
        this.data.Add("distance", data["distance"].ToObject<int>());
        this.data.Add("speed", data["speed"].ToObject<int>());
        this.data.Add("heartrate", data["heartRate"].ToObject<int>());
        this.data.Add("elapsed", data["elapsed"].ToObject<int>());
        this.data.Add("deviceType", data["deviceType"].ToObject<string>());
        this.data.Add("id", data["id"].ToObject<string>());
    }
}