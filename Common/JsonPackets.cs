using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Common;


public abstract class DAbstract
{
    public string ToJson() {
        return JsonConvert.SerializeObject(this);
    }
}


/*public class JsonFile
{
    public int StatusCode { get; set; }
    public string OppCode { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public JsonData Data { get; set; }
}

public class JsonData
{
    public uint AutenticationID { get; set; }
    public string Password { get; set; }
    public string Title { get; set; }
    public string FileName { get; set; }
    public string ChatMessage { get; set; }
    public string Content { get; set; }
    public bool BikeDataHistory { get; set; }
}*/