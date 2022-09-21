using RemoteHealthcare.Data.Providers;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket;

try
{
    
    var engine = new EngineConnection();

    await engine.ConnectAsync("nickw");
    var bike = await DataProvider.GetBike("24517");
    var heart = await DataProvider.GetHeart();
    var connection = new VrConnection(bike, heart, engine);
    connection.start();

    await Task.Delay(-1);

    
}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}