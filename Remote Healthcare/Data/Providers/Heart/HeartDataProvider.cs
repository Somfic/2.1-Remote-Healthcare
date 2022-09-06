using System.Threading.Tasks;

namespace RemoteHealthcare.Data.Providers.Heart;

public abstract class HeartDataProvider : IDataProvider<HeartData>
{
    private readonly HeartData _data = new();

    protected void SetHeartRate(int heartRate)
    {
        _data.HeartRate = heartRate;
    }

    protected void SetId(string id)
    {
        _data.Id = id;
    }

    public HeartData GetData()
    {
        return _data;
    }

    public abstract Task Process();
}