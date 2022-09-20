using Newtonsoft.Json;

namespace RemoteHealthcare.Socket.Models.Response;

public class SessionList : IDataResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("beginTime")] public DateTime BeginTime { get; set; }

    [JsonProperty("lastPing")] public DateTime LastPing { get; set; }

    [JsonProperty("fps")] public FrameRate[] Frames { get; set; }

    [JsonProperty("features")] public string[] Features { get; set; }
    [JsonProperty("clientinfo")] public ClientInfo Client { get; set; }


    public class ClientInfo
    {
        [JsonProperty("host")] public string Host { get; set; }
        [JsonProperty("user")] public string User { get; set; }

        [JsonProperty("file")] public string File { get; set; }

        [JsonProperty("renderer")] public string Renderer { get; set; }
    }

    public class FrameRate
    {
        [JsonProperty("time")] public double Time { get; set; }

        [JsonProperty("fps")] public double Fps { get; set; }
    }
}