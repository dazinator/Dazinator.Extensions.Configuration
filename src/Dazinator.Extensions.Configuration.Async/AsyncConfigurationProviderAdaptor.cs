namespace Dazinator.Extensions.Configuration.Tests;

using System.Reflection.Metadata;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

public class AsyncConfigurationProviderAdaptor : ConfigurationProvider, IDisposable
{
    private readonly IAsyncConfigurationProvider _asyncProvider;
    private readonly bool _disposeAsyncProviderOnDispose;
    private readonly IDisposable _changeTokenRegistration;

    public AsyncConfigurationProviderAdaptor(IAsyncConfigurationProvider asyncProvider, bool disposeAsyncProviderOnDispose = false)
    {
        _asyncProvider = asyncProvider;
        _disposeAsyncProviderOnDispose = disposeAsyncProviderOnDispose;
        var changeTokenProducer = () => _asyncProvider.GetReloadToken();
        _changeTokenRegistration = changeTokenProducer.OnChange(async () => {            
            Data = await _asyncProvider.LoadAsync().ConfigureAwait(false);
            OnReload(); // handles the notification of changes to OptionsMonitor.
        } );
    }

   

    public override void Load()
    {
        base .Load();
        // Initially we set empty dictionary, then we will load it async.
        var dic = new Dictionary<string, string>();
        Data = dic;        
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

public interface IAsyncConfigurationProvider
{
    Task<IDictionary<string, string>> LoadAsync();

    IChangeToken GetReloadToken();
}

public class DelegateAsyncConfigurationProvider : IAsyncConfigurationProvider
{
    private readonly Func<IChangeToken> _changeTokenProducer;
    private readonly Func<Task<IDictionary<string, string>>> _onLoadAsync;

    public DelegateAsyncConfigurationProvider(Func<IChangeToken> changeTokenProducer, Func<Task<IDictionary<string, string>>> onLoadAsync)
    {
        _changeTokenProducer = changeTokenProducer;
        _onLoadAsync = onLoadAsync;
    }

    public IChangeToken GetReloadToken() => _changeTokenProducer();
    public Task<IDictionary<string, string>> LoadAsync() => _onLoadAsync();
}

