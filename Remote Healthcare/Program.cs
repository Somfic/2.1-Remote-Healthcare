using RemoteHealthcare.Client;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Data.Providers;
using RemoteHealthcare.Socket;

try
{

    var engine = new EngineConnection();
    await engine.ConnectAsync();


    var bike = await DataProvider.GetBike("00472");
    var heart = await DataProvider.GetHeart();

    var vrConnection = new VrConnection(bike, heart, engine);
    vrConnection.start();

    var client = new Client();



    await Task.Delay(-1);



}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}