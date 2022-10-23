using Newtonsoft.Json;

namespace RemoteHealthcare.GUIs.Patient.Simulation.Models;

public class Configration
{
    [JsonProperty("host")]
    public string Host { get; init; }
    
    [JsonProperty("port")]
    public int Port { get; init; }
}