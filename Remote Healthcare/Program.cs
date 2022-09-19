using RemoteHealthcare.Data.Providers;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket;

try
{
    var engine = new EngineConnection();
    await engine.ConnectAsync();

    var bike = await DataProvider.GetBike("00438");
    var heart = await DataProvider.GetHeart();

    await Task.Delay(-1);
}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}