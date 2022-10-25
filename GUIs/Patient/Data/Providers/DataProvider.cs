using System;
using System.Threading.Tasks;
using RemoteHealthcare.Client.Data.Providers.Bike;
using RemoteHealthcare.Client.Data.Providers.Heart;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.GUIs.Patient.Data.Providers.Bike;

namespace RemoteHealthcare.Client.Data.Providers;

public static class DataProvider
{
    private static readonly Log Log = new(typeof(DataProvider));

    public static async Task<HeartDataProvider> GetHeart()
    {
        try
        {
            var provider = new BluetoothHeartDataProvider();
            await provider.Initialise();
            return provider;
        }
        catch (Exception)
        {
            Log.Information("Switching to simulated heart provider");
            var provider = new SimulationHeartDataProvider();
            await provider.Initialise();
            return provider;
        }
    }

    public static async Task<BikeDataProvider> GetBike(string serial)
    {
        try
        {
            var provider = new BluetoothBikeDataProvider(serial);
            await provider.Initialise();
            return provider;
        }
        catch (Exception)
        {
            Log.Information("Switching to simulated bike provider");
            var provider = new SimulationBikeDataProvider();
            await provider.Initialise();
            return provider;
        }
    }
}