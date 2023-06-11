namespace Dazinator.Extensions.Configuration.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Shouldly;
using Xunit.Categories;

[UnitTest]
public class AsyncConfigurationProviderTests
{

    [Fact]
    public async Task CanAddAsyncDelegateProvider()
    {

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddAsyncProvider((source) =>
        {
            source.ChangeTokenProducer = () => EmptyChangeToken.Instance; // trigger your change token to cause a reload.
            source.OnLoadConfigurationAsync = LoadConfigurationAsync;
        });

        configurationBuilder.Sources.Count.ShouldBe(1);
    }

    private Task<IDictionary<string, string>> LoadConfigurationAsync()
    {
        var result = new Dictionary<string, string>()
        {
            { "key1", "value1" }
        };
        return Task.FromResult<IDictionary<string, string>>(result);
    }

    [Fact]
    public async Task CanAddAsyncProvider()
    {

        var testProvider = new TestAsyncConfigurationProvider();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddAsyncProvider(testProvider, disposeWhenConfigurationDisposed: true); // if false (default), you are responsible for disposing of the provider.
        configurationBuilder.Sources.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CanReloadAsync()
    {
        var triggerTokenCtsSource = new CancellationTokenSource();
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddAsyncProvider((source) =>
        {

            var changeTokenProducer = () => new CancellationChangeToken(triggerTokenCtsSource.Token);
            source.ChangeTokenProducer = changeTokenProducer;

            source.OnLoadConfigurationAsync = async () =>
            {
                await Task.Yield();
                return new Dictionary<string, string>()
                {
                    { "key1", "value1" }
                };
            };
        });

        var configuration = configurationBuilder.Build();
        var setting = configuration["key1"];
        setting.ShouldBeNull();

        triggerTokenCtsSource.Cancel();
        await Task.Delay(200);

        setting = configuration["key1"];
        setting.ShouldBe("value1");

    }



    public class TestAsyncConfigurationProvider : IAsyncConfigurationProvider, IDisposable
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
}
