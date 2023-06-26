namespace Dazinator.Extensions.Configuration.Async;

using Microsoft.Extensions.Configuration;

public interface IAsyncConfigurationSource : IConfigurationSource
{
    public Task InitialiseAsync();
}
