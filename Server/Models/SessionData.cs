using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class SessionData
{
    public string Id { get; set; }
    public Dictionary<string, dynamic> Data { get; set; }

    public SessionData(JObject data)
    {
        Id = data["sessionId"].ToObject<string>();
        Data = new Dictionary<string, dynamic>();
        SetupDictionary(data);
    }

    private void SetupDictionary(JObject data)
    {
        Data.Add("distance", data["distance"].ToObject<int>());
        Data.Add("speed", data["speed"].ToObject<int>());
        Data.Add("heartrate", data["heartRate"].ToObject<int>());
        Data.Add("elapsed", data["elapsed"].ToObject<int>());
        Data.Add("deviceType", data["deviceType"].ToObject<string>());
        Data.Add("id", data["id"].ToObject<string>());
    }
}