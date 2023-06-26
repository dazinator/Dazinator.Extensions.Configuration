namespace Dazinator.Extensions.Configuration.AdoNet;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

public interface IAsyncItemProvider<TItem>
{
    Task<TItem> LoadAsync();

    IChangeToken GetReloadToken();
}
