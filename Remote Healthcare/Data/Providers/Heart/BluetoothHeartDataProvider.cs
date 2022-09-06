namespace RemoteHealthcare.Data.Providers.Heart;

public class BluetoothHeartDataProvider : HeartDataProvider
{
    public override async Task Process()
    {
        await Task.CompletedTask;
    }
}