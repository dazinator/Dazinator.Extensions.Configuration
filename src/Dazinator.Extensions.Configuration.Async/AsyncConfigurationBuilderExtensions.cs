namespace Dazinator.Extensions.Configuration.Tests;

using Dazinator.Extensions.Configuration.Async;
using Microsoft.Extensions.Configuration;

public static class AsyncConfigurationBuilderExtensions
{
    public static Task<IConfigurationBuilder> AddProviderAsync(this IConfigurationBuilder builder,
        Action<DelegateAsyncProviderOptions> configureOptions)
    {
        var options = new DelegateAsyncProviderOptions();
        configureOptions?.Invoke(options);
        var asyncProvider = new DelegateAsyncConfigurationProvider(options.ChangeTokenProducer, options.OnLoadConfigurationAsync);
        return AddProviderAsync(builder, asyncProvider, disposeWhenConfigurationDisposed: false);
    }

    public static async Task<IConfigurationBuilder> AddProviderAsync(this IConfigurationBuilder builder,
        IAsyncConfigurationProvider provider,
        bool disposeWhenConfigurationDisposed = false)
    {
        var source = new AsyncConfigurationProviderSource(provider, disposeWhenConfigurationDisposed);
        await source.InitialiseAsync();
        return builder.Add(source);
    }

}
