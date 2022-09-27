using Newtonsoft.Json;

namespace NetworkEngine.Socket.Models;

public class DataResponse<TData> where TData : Response.IDataResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("data")] public TData? Data { get; set; }
}