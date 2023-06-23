## Problem

This repository is home to a number of libraries that build upon `Microsoft.Extensions.Configuration` including a SQL Server provider, and an Async provider.

Why a Sql server provider? https://gamma.app/docs/Custom-dotnet-configuration-provider-for-SQL-Server-with-realtime-ad56s549my9xtxq


### Async

Implements all the boilder plate, and allows you to provide your own `asynchronous` configuration, with support for reloads in a simple way.
This library allows you to just to focus on providing the actual functionnality you are interested in.

To use this library you just call `AddAsyncProvider` when building configuration. There are a couple of overloads:

Supply functions:

```csharp
  var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddAsyncProvider((source) =>
        {
            source.ChangeTokenProducer = () => EmptyChangeToken.Instance; // provide a change token, which when signalled will cause OnLoadConfigurationAsync to be invoked again.
            source.OnLoadConfigurationAsync = LoadConfigurationAsync; // provide a method to load the latest configuration, asynchronously.
        });

```

Or if you prefer to implement a class, you can supply an instance of `IAsyncConfigurationProvider`:

```csharp
  var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.AddAsyncProvider(new MyAsyncConfigurationProvider());
```

Where `MyAsyncConfigurationProvider` implements `IAsyncConfigurationProvider` and also optionally imlmenets `IDisposable`.

```csharp

    public class MyAsyncConfigurationProvider : IAsyncConfigurationProvider, IDisposable
    {
        public void Dispose()
        {
            // you can optionally implement IDisposable interface
        }

        public IChangeToken GetReloadToken() => EmptyChangeToken.Instance;
        public async Task<IDictionary<string, string>> LoadAsync()
        {
            await Task.Yield();

            var result = new Dictionary<string, string>()
            {
                { "key1", "value1" }
            };
            return result;
        }
    }
```