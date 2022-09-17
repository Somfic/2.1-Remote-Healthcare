﻿using RemoteHealthcare.Data.Providers;
using RemoteHealthcare.Logger;
using RemoteHealthcare.Socket;

try
{
    var engine = new EngineConnection();
    await engine.ConnectAsync("richa");

    var bike = await DataProvider.GetBike();
    await bike.Initialise();

    var heart = await DataProvider.GetHeart();
    await heart.Initialise();

    await Task.Delay(-1);
}
catch (Exception ex)
{
    var log = new Log(typeof(Program));
    log.Critical(ex, "Program stopped because of exception");
}