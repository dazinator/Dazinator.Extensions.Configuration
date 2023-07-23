namespace Dazinator.Extensions.Configuration.Tests.Async;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.Async;
using Dazinator.Extensions.Configuration.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Shouldly;
using Xunit.Categories;

[UsesVerify]
[UnitTest]
public class AsyncConfigurationProviderTests
{

    [Fact]
    public async Task CanAddAsyncDelegateProvider()
    {

        var configurationBuilder = new ConfigurationBuilder();
        await configurationBuilder.AddProviderAsync((source) =>
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
        await configurationBuilder.AddProviderAsync(testProvider, disposeWhenConfigurationDisposed: true); // if false (default), you are responsible for disposing of the provider.
        configurationBuilder.Sources.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CanInitialiseAsync()
    {
        var triggerTokenCtsSource = new CancellationTokenSource();
        var configurationBuilder = new ConfigurationBuilder();

        await configurationBuilder.AddProviderAsync((source) =>
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
        setting.ShouldBe("value1");
    }

    [Fact]
    public async Task CanReloadAsync()
    {
        CancellationTokenSource triggerTokenCtsSource = null;
        var changeTokenProducer = () =>
        {

            var newSource = new CancellationTokenSource();
            var previousSource = Interlocked.Exchange(ref triggerTokenCtsSource, newSource);
            previousSource?.Dispose();
            return new CancellationChangeToken(triggerTokenCtsSource.Token);
        };


        var loadCount = 0;
        var configurationBuilder = new ConfigurationBuilder();
        await configurationBuilder.AddProviderAsync((source) =>
         {
             source.ChangeTokenProducer = changeTokenProducer;

             source.OnLoadConfigurationAsync = async () =>
             {
                 loadCount++;
                 await Task.Yield();

                 return new Dictionary<string, string>()
                 {
                    { "key1", $"value{loadCount}" }
                 };
             };
         });

        var configuration = configurationBuilder.Build();
        var setting = configuration["key1"];
        setting.ShouldBe("value1");

        var signalled = false;


        Func<IChangeToken> configChanged = configuration.GetReloadToken;
        triggerTokenCtsSource.Cancel(); // trigger reload.

        await configChanged.WaitOneAsync();

        // using var registration = ChangeToken.OnChange(() => configuration.GetReloadToken(), () => signalled = true);
        // get notified when reload has occurred.
       // var reloadToken = configuration.GetReloadToken();


      //  var mockOptionsSource = new Mock<IOptionsChangeTokenSource>();
      //  mockOptionsSource.Setup(x => x.GetChangeToken()).Returns(() => configuration.GetReloadToken());

       // mockAdaptor.Setup(x => x.LoadAsync()).Returns(GetConfigurationItemsSetOne);
        //var mockChangeSournce = new Mock<IOptionsChangeTokenSource>()
        //  await Task.Delay(200);

        //  await Verify(configuration);

        setting = configuration["key1"];
        var u = setting;
        setting.ShouldBe("value2");
        //signalled.ShouldBeTrue();
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

        CancellationTokenSource triggerTokenCtsSource = null;
        var changeTokenProducer = () =>
        {

            var newSource = new CancellationTokenSource();
            var previousSource = Interlocked.Exchange(ref triggerTokenCtsSource, newSource);
            previousSource?.Dispose();
            return new CancellationChangeToken(triggerTokenCtsSource.Token);
        };

        var configurationBuilder = new ConfigurationBuilder();
        var loadCount = 0;
        await configurationBuilder.AddProviderAsync((source) =>
        {

            source.ChangeTokenProducer = changeTokenProducer;

            source.OnLoadConfigurationAsync = async () =>
            {
                loadCount++;
                await Task.Yield();

                return new Dictionary<string, string>()
                 {
                    { "Key1", $"value{loadCount}" }
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
            options.CurrentValue.Key1.ShouldBe("value1");

            triggerTokenCtsSource.Cancel();

            await Task.Delay(200);

            options.CurrentValue.Key1.ShouldBe("value2");
        }
    }

    public class MyOptions
    {
        public string Key1 { get; set; }
    }

}
