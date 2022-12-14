using System;
using System.Threading.Tasks;

namespace RemoteHealthcare.Common.Data.Providers.Heart;

public class SimulationHeartDataProvider : HeartDataProvider
{
    public override Task Initialise()
    {
        return Task.CompletedTask;
    }

    public override async Task ProcessRawData()
    {
        SetId("Simulation");
        SetHeartRate(new Random().Next(60, 100));
    }
}