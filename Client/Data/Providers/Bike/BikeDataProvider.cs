namespace RemoteHealthcare.Data.Providers.Bike;

public abstract class BikeDataProvider : IDataProvider<BikeData>
{
    private readonly BikeData _data = new();

    public abstract Task Initialise();

    public abstract Task ProcessRawData();

    public abstract void SentMessage(byte[] bytes);

    public BikeData GetData()
    {
        return _data;
    }

    protected void SetDistance(float distance)
    {
        _data.Distance = distance;
    }

    protected void SetSpeed(float speed)
    {
        _data.Speed = speed;
    }

    protected void SetHeartRate(int heartRate)
    {
        _data.HeartRate = heartRate;
    }

    protected void SetElapsed(TimeSpan elapsed)
    {
        _data.Elapsed = elapsed;
    }

    protected void SetTotalElapsed(TimeSpan elapsed)
    {
        _data.TotalElapsed = elapsed;
    }

    protected void SetDeviceType(DeviceType deviceType)
    {
        _data.DeviceType = deviceType;
    }

    protected void SetId(string id)
    {
        _data.Id = id;
    }

}