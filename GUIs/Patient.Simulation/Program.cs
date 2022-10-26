using Newtonsoft.Json;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.GUIs.Patient.Simulation;
using RemoteHealthcare.GUIs.Patient.Simulation.Models;

var log = new Log(typeof(Program));

// Read the configration file
var configContent = File.ReadAllText("config.json");
var config = JsonConvert.DeserializeObject<Configuration>(configContent);

// Find out how many clients to simulate
var amountOfClients = 0;
while(true) {
    log.Information("Enter amount of patients to simulate");
    if (!int.TryParse(Console.ReadLine(), out amountOfClients)) {
        log.Error("Invalid input");
        continue;
    }
    
    if (amountOfClients < 1) {
        log.Error("Amount of patients must be greater than 0");
        continue;
    }
    
    break;
}

// Create the clients
List<SimulatedClient> clients = new();
for (var i = 0; i < amountOfClients; i++)
    clients.Add(new SimulatedClient($"#{i}"));

// Connect the clients to the server
log.Information($"Connecting to server on {config.Host}:{config.Port}");
try
{
    foreach (var client in clients)
        await client.ConnectAsync(config.Host, config.Port);
}
catch (Exception ex)
{
    log.Critical(ex, "Not all clients could not connect to the server");
    return;
}

// Login the clients
log.Information("Logging in clients");
try
{
    foreach (var client in clients)
    {
        await client.LoginAsync();
        await Task.Delay(500);
    }
}
catch (Exception ex)
{
    log.Critical(ex, "Not all clients could not login to the server");
    return;
}

while (true)
{
    await Task.Delay(1000);
    foreach (var client in clients)
    {
        await client.SendBikeData();
    }
}