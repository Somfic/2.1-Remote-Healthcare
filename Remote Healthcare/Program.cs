using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Data.Providers;
using RemoteHealthcare.Socket;

try
{

    var engine = new EngineConnection();

    await engine.ConnectAsync();
    Task.Delay(2000);
    var bike = await DataProvider.GetBike("00472");
    var heart = await DataProvider.GetHeart();
    Task.Delay(2000);
    //var connection = new VrConnection(bike, heart, engine);
    //connection.start();

    await Task.Delay(-1);



}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}