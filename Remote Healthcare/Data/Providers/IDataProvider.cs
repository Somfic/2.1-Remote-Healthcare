using System.Threading.Tasks;

namespace RemoteHealthcare.Data.Providers;

public interface IDataProvider<out TData> where TData : IData
{
    TData GetData();

    Task Process();
}