using Newtonsoft.Json;

namespace RemoteHealthcare.Socket.Models.Response;

public class TunnelCreate : IDataResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("status")] public string Status { get; set; }
}