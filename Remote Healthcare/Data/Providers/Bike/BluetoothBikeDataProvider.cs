using System.Threading.Tasks;

namespace RemoteHealthcare.Data.Providers.Bike;

public class BluetoothBikeDataProvider : BikeDataProvider
{
    public override async Task Process()
    {
        await Task.CompletedTask;
    }
}