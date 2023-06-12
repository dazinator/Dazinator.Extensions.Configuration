namespace Dazinator.Extensions.Configuration.Tests;
using Microsoft.Extensions.Primitives;

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

