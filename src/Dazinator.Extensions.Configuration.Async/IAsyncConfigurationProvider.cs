namespace Dazinator.Extensions.Configuration.Async;
using Microsoft.Extensions.Primitives;

public interface IAsyncConfigurationProvider
{
    Task<IDictionary<string, string>> LoadAsync();

    IChangeToken GetReloadToken();
}

