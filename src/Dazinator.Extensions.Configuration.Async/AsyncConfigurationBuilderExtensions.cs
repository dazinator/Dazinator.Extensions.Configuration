namespace Dazinator.Extensions.Configuration.Tests;

using Microsoft.Extensions.Configuration;

public static class AsyncConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddAsyncProvider(this IConfigurationBuilder builder,
        Action<DelegateAsyncProviderOptions> configureOptions)
    {
        var options = new DelegateAsyncProviderOptions();
        configureOptions?.Invoke(options);
        var asyncProvider = new DelegateAsyncConfigurationProvider(options.ChangeTokenProducer, options.OnLoadConfigurationAsync);
        var source = new AsyncConfigurationProviderSource(asyncProvider);
        return builder.Add(source);
    }

    public static IConfigurationBuilder AddAsyncProvider(this IConfigurationBuilder builder, IAsyncConfigurationProvider provider, bool disposeWhenConfigurationDisposed = false)
    {
        var source = new AsyncConfigurationProviderSource(provider, disposeWhenConfigurationDisposed);
        return builder.Add(source);
    }

}
