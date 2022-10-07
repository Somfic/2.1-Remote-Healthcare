using NetworkEngine.Socket;
using RemoteHealthcare.Client;

using RemoteHealthcare.Client.Data.Providers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.NetworkEngine;

try
{

    var engine = new EngineConnection();
    await engine.ConnectAsync("cave");

    Console.WriteLine("Enter Bike ID:");
    var bike = await DataProvider.GetBike(Console.ReadLine());
    var heart = await DataProvider.GetHeart();
    var vrConnection = new VrConnection(bike, heart, engine);
    vrConnection.Start();


    var client = new Client(vrConnection);
    client.RunAsync();
    await Task.Delay(-1);
}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}