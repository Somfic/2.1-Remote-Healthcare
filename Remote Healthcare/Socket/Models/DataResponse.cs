﻿using Newtonsoft.Json;
using RemoteHealthcare.Socket.Models.Response;

namespace RemoteHealthcare.Socket.Models;

public class DataResponse<TData> where TData : IDataResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("data")] public TData? Data { get; set; }
}