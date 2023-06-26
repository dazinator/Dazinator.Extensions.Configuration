namespace Dazinator.Extensions.Configuration.Async;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class AsyncConfigurationProviderSource : IAsyncConfigurationSource
{
    private readonly IAsyncConfigurationProvider _provider;
    private readonly bool _disposeWhenConfigurationDisposed;
    private IDictionary<string, string> _initialData;

    public AsyncConfigurationProviderSource(IAsyncConfigurationProvider provider, bool disposeWhenConfigurationDisposed = false)
    {
        _provider = provider;
        _disposeWhenConfigurationDisposed = disposeWhenConfigurationDisposed;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var adaptor = new AsyncConfigurationProviderAdaptor(_provider, _initialData, _disposeWhenConfigurationDisposed);
        return adaptor;
    }

    /// <summary>
    /// This should be invoked in advance of building the configuration, to allow the provider to load its configuration asynchronousl so that is immediately available when the configuration is built.
    /// </summary>
    /// <returns></returns>
    public async Task InitialiseAsync() =>
        /// trigger the async provider to load its intial configuration asynchronously.
        _initialData = await _provider.LoadAsync();
}
