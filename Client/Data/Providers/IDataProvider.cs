namespace RemoteHealthcare.Data.Providers;

public interface IDataProvider<out TData> where TData : IData
{
    Task Initialise();

    TData GetData();

    Task ProcessRawData();
}