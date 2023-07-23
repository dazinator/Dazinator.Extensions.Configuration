namespace Dazinator.Extensions.Configuration.Async;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

public interface IAsyncConfigurationDataProvider<TItem>
{
    Task<TItem> LoadAsync();

    IChangeToken GetReloadToken();
}
