using Newtonsoft.Json;

namespace RemoteHealthcare.NetworkEngine.Socket.Models;

public class DataResponses<TData> where TData : Response.IDataResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("data")] public TData[]? Data { get; set; }
}