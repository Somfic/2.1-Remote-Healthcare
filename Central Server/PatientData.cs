using Newtonsoft.Json.Linq;

namespace Server;

public class PatientData
{
    public string Id { get; set; }
    public string password { get; set; }
    public Dictionary<string, dynamic> data { get; set; }

    public PatientData(string id, string password, JObject data)
    {
        this.Id = id;
        this.password = password;
        SetupDictionary(data);
    }

    private void SetupDictionary(JObject data)
    {
        this.data.Add("distance", data["distance"].ToObject<int>());
        this.data.Add("speed", data["speed"].ToObject<int>());
        this.data.Add("heartrate", data["heartRate"].ToObject<int>());
        this.data.Add("elapsed", data["elapsed"].ToObject<int>());
        this.data.Add("deviceType", data["deviceType"].ToObject<int>());
        this.data.Add("id", data["id"].ToObject<int>());
        this.data.Add("sessionId", data["sessionId"].ToObject<int>());
    }
}