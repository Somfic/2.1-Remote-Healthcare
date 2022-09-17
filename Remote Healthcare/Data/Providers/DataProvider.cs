using RemoteHealthcare.Data.Providers.Bike;
using RemoteHealthcare.Data.Providers.Heart;
using RemoteHealthcare.Logger;

namespace RemoteHealthcare.Data.Providers;

public static class DataProvider
{
    private static readonly Log Log = new(typeof(DataProvider));
	
    public static async Task<HeartDataProvider> GetHeart()
    {
        try
        {
            var provider = new SimulationHeartDataProvider();
            await provider.Initialise();
            return provider;
        }
        catch (PlatformNotSupportedException)
        {
            Log.Debug("Switching to simulated heart provider");
            var provider = new SimulationHeartDataProvider();
            await provider.Initialise();
            return provider;
        }
    }

    public static async Task<BikeDataProvider> GetBike()
    {
        try
        {
            var provider = new SimulationBikeDataProvider();
            await provider.Initialise();
            return provider;
        }
        catch (PlatformNotSupportedException)
        {
            Log.Debug("Switching to simulated bike provider");
            var provider = new SimulationBikeDataProvider();
            await provider.Initialise();
            return provider;
        }
    }
}