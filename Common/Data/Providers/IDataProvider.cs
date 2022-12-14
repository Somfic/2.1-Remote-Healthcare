using System.Threading.Tasks;

namespace RemoteHealthcare.Common.Data.Providers;

public interface IDataProvider<out TData> where TData : IData
{
    Task Initialise();

    TData GetData();

    Task ProcessRawData();
}