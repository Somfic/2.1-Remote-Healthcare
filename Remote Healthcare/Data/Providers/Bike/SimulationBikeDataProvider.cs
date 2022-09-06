using System.Diagnostics;
using System.Threading.Tasks;

namespace RemoteHealthcare.Data.Providers.Bike;

public class SimulationBikeDataProvider : BikeDataProvider
{
    private readonly Random _random = new();
    private readonly Stopwatch _timer = Stopwatch.StartNew();
    private readonly Stopwatch _totalElapsed = Stopwatch.StartNew();

    private int _totalReset;

    public override async Task Process()
    {
        // Id
        SetId("Simulation");
        
        // Device type
        SetDeviceType(DeviceType.Bike);
        
        // Elapsed
        if (_timer.Elapsed.TotalSeconds >= 64)
            _timer.Restart();
        
        SetElapsed(_timer.Elapsed); // todo: rond af op 0,25 seconden
        
        // Total elapsed
        SetTotalElapsed(_totalElapsed.Elapsed); ;
        
        // Speed
        var deltaSpeed = _random.NextSingle() * 2f - 1f;
        var newSpeed = GetData().Speed + deltaSpeed;
        SetSpeed(Math.Clamp(newSpeed, 5, 234));
        
        // Distance
        var newDistance = (GetData().Distance + GetData().Speed * 0.25f) % 256;
        SetDistance(newDistance);
        
        // Heart rate
        SetHeartRate((int)Math.Round(newSpeed + 80));
    }
}