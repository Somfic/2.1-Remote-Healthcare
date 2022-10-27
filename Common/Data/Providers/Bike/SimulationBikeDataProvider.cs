using System.Diagnostics;

namespace RemoteHealthcare.Common.Data.Providers.Bike;

public class SimulationBikeDataProvider : BikeDataProvider
{
    private readonly Random _random = new();
    private readonly Stopwatch _timer = Stopwatch.StartNew();
    private readonly Stopwatch _timerSinceLastProcess = Stopwatch.StartNew();
    private readonly Stopwatch _totalElapsed = Stopwatch.StartNew();

    private int _totalReset;

    public override Task Initialise()
    {
        return Task.CompletedTask;
    }

    public override async Task ProcessRawData()
    {
        // Id
        SetId("Simulation");

        // Device type
        SetDeviceType(DeviceType.Bike);

        // Elapsed
        if (_timer.Elapsed.TotalSeconds >= 64)
            _timer.Restart();

        SetElapsed(_timer.Elapsed);

        // Total elapsed
        SetTotalElapsed(_totalElapsed.Elapsed);
        ;

        // Speed
        var deltaSpeed = _random.NextDouble() * 2f - 1f;
        var newSpeed = GetData().Speed + deltaSpeed;
        SetSpeed((float)Math.Max(5, Math.Min(234, newSpeed)));

        // Distance
        var newDistance = (float)(GetData().Distance + GetData().Speed * _timerSinceLastProcess.Elapsed.TotalSeconds);
        SetDistance(newDistance); 

        // Heart rate
        SetHeartRate((int)Math.Round(newSpeed + 80));

        _timerSinceLastProcess.Restart();
    }

    public override Task SendMessage(byte[] bytes)
    {
        return Task.CompletedTask;
    }
}