namespace Dazinator.Extensions.Configuration.Tests.Async;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.AdoNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Categories;


[UsesVerify]
[UnitTest]
public class JsonColumnAsyncConfigurationProviderTests
{

    [Fact]
    public Task LoadAsync_ReturnsExpectedConfiguration()
    {
        var options = new JsonColumnAsyncConfigurationProviderOptions
        {
            GetConfigurationItems = GetConfigurationItemsSetOne
        };

        //  options.GetChangeTokenProducer = () => EmptyChangeToken.Instance;
        var services = new ServiceCollection();
        services.AddLogging();
        using var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<JsonItemAsyncConfigurationProvider>>();

        using var sut = new JsonItemAsyncConfigurationProvider(options, logger);
        var results = sut.LoadAsync();

        return Verify(results);
    }

    private async Task<IList<JsonConfigurationItem>> GetConfigurationItemsSetOne()
    {
        var results = new List<JsonConfigurationItem>();
        for (var i = 1; i <= 10; i++)
        {
            var result = new JsonConfigurationItem()
            {
                configSectionPath = $"key{i}",
                json = "{\"foo\": \"bar\"}"
            };

            //  await Task.Delay(1000);
            results.Add(result);
        }
        return results;
    }

}
