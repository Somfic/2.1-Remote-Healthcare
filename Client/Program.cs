using NetworkEngine.Socket;
using RemoteHealthcare.Client.Client;
using RemoteHealthcare.Client.Data.Providers;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.NetworkEngine;

try
{
    var engine = new EngineConnection();
    await engine.ConnectAsync();

    Console.WriteLine("Enter Bike ID:");
    var bike = await DataProvider.GetBike(Console.ReadLine());
    var heart = await DataProvider.GetHeart();

    var vr = new VrConnection(bike, heart, engine);
    vr.Start();

    var client = new Client(vr);
    client.RunAsync();
    await Task.Delay(-1);
}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}