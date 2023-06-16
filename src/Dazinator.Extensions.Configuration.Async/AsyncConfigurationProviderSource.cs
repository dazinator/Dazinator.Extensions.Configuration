namespace Dazinator.Extensions.Configuration.Async;

using Microsoft.Extensions.Configuration;


public class AsyncConfigurationProviderSource : IConfigurationSource
{
    private readonly IAsyncConfigurationProvider _provider;
    private readonly bool _disposeWhenConfigurationDisposed;

    public AsyncConfigurationProviderSource(IAsyncConfigurationProvider provider, bool disposeWhenConfigurationDisposed = false)
    {
        _provider = provider;
        _disposeWhenConfigurationDisposed = disposeWhenConfigurationDisposed;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var adaptor = new AsyncConfigurationProviderAdaptor(_provider, _disposeWhenConfigurationDisposed);
        return adaptor;
    }
}
