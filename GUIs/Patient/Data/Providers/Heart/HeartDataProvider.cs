using System.Threading.Tasks;

namespace RemoteHealthcare.Client.Data.Providers.Heart;

public abstract class HeartDataProvider : IDataProvider<HeartData>
{
    private readonly HeartData _data = new();

    public HeartData GetData()
    {
        return _data;
    }

    public abstract Task Initialise();

    public abstract Task ProcessRawData();

    protected void SetHeartRate(int heartRate)
    {
        _data.HeartRate = heartRate;
    }

    protected void SetId(string id)
    {
        _data.Id = id;
    }
}