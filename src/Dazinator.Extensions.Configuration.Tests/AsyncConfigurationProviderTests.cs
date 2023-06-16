namespace Dazinator.Extensions.Configuration.Tests;

using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.Async;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shouldly;
using Xunit.Abstractions;
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

        bool signalled = false;
        using var registration = ChangeToken.OnChange(() => configuration.GetReloadToken(), () => signalled = true);

        triggerTokenCtsSource.Cancel();
        await Task.Delay(200);

        setting = configuration["key1"];
        setting.ShouldBe("value1");
        signalled.ShouldBeTrue();
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

    [Fact]
    public async Task CanUseOptionsMonitor()
    {

        var configurationBuilder = new ConfigurationBuilder();
        using var cts = new CancellationTokenSource();

        configurationBuilder.AddAsyncProvider((source) =>
        {

            var changeTokenProducer = () =>
            {
                return new CancellationChangeToken(cts.Token);
            };

            source.ChangeTokenProducer = changeTokenProducer;

            source.OnLoadConfigurationAsync = async () =>
            {
                await Task.Yield();
                return new Dictionary<string, string>()
                {
                    { "Key1", "value1" }
                };
            };
        });

        var configuration = configurationBuilder.Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<MyOptions>(configuration);


        var sp = services.BuildServiceProvider();
        using (var scope = sp.CreateScope())
        {
            var options = sp.GetRequiredService<IOptionsMonitor<MyOptions>>();

            options.CurrentValue.ShouldNotBeNull();
            options.CurrentValue.Key1.ShouldBeNull();

            cts.Cancel();

            await Task.Delay(200);

            options.CurrentValue.Key1.ShouldBe("value1");
        }
    }

    public class MyOptions
    {
        public string Key1 { get; set; }
    }

}
