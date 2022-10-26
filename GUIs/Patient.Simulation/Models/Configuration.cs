using Newtonsoft.Json;

namespace RemoteHealthcare.GUIs.Patient.Simulation.Models;

public class Configuration
{
    [JsonProperty("host")]
    public string Host { get; init; }
    
    [JsonProperty("port")]
    public int Port { get; init; }
    
    [JsonProperty("username")]
    public string Username { get; init; }
    
    [JsonProperty("password")]
    public string Password { get; init; }
}