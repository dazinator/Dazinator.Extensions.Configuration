namespace Dazinator.Extensions.Configuration.Tests;
using Microsoft.Extensions.Primitives;

public interface IAsyncConfigurationProvider
{
    Task<IDictionary<string, string>> LoadAsync();

    IChangeToken GetReloadToken();
}

