namespace Dazinator.Extensions.Configuration.Async;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

public class AsyncConfigurationProviderAdaptor : ConfigurationProvider, IDisposable
{
    private readonly IAsyncConfigurationProvider _asyncProvider;
    // private readonly Dictionary<string, string> _initialConfig;
    private readonly bool _disposeAsyncProviderOnDispose;
    private readonly IDisposable _changeTokenRegistration;

    public AsyncConfigurationProviderAdaptor(
        IAsyncConfigurationProvider asyncProvider,
        IDictionary<string, string> initialConfig,
        bool disposeAsyncProviderOnDispose = false)
    {
        _asyncProvider = asyncProvider;
        Data = initialConfig ?? Data;
        _disposeAsyncProviderOnDispose = disposeAsyncProviderOnDispose;
        var changeTokenProducer = () => _asyncProvider.GetReloadToken();
        _changeTokenRegistration = changeTokenProducer.OnChange(async () =>
        {
            Data = await _asyncProvider.LoadAsync().ConfigureAwait(false);
            OnReload(); // handles the notification of changes to OptionsMonitor.
        });
    }

    public void Dispose()
    {
        _changeTokenRegistration?.Dispose();
        if (_disposeAsyncProviderOnDispose && _asyncProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

