using RemoteHealthcare.Client.Client;
using RemoteHealthcare.Common.Logger;

try
{
    /*
    var engine = new EngineConnection();
    await engine.ConnectAsync("cave");
    
    Console.WriteLine("Enter Bike ID:");
    var bike = await DataProvider.GetBike(Console.ReadLine());
    var heart = await DataProvider.GetHeart();
    */

    var client = new Client();
    client.RunAsync();
    await Task.Delay(-1);
}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}