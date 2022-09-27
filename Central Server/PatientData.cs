using Newtonsoft.Json.Linq;

namespace Server;

public class PatientData
{
    public string Id { get; set; }
    public JObject data { get; set; }

    public PatientData(string id, JObject data)
    {
        this.Id = id;
        this.data = data;
    }
}