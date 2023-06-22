namespace Dazinator.Extensions.Configuration.Tests.Async;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Categories;
using Dazinator.Extensions.Configuration.AdoNet;
using Microsoft.Extensions.Logging;


[UsesVerify]
[UnitTest]
public class JsonColumnAsyncConfigurationProviderTests
{

    [Fact]
    public Task LoadAsync_ReturnsExpectedConfiguration()
    {
        var options = new JsonColumnAsyncConfigurationProviderOptions();
        options.GetConfigurationItems = GetConfigurationItems;

        //  options.GetChangeTokenProducer = () => EmptyChangeToken.Instance;
        var services = new ServiceCollection();
        services.AddLogging();
        using var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<JsonColumnAsyncConfigurationProvider>>();

        using var sut = new JsonColumnAsyncConfigurationProvider(options, logger);
        var results = sut.LoadAsync();

        return Verify(results);
    }

    private async IAsyncEnumerable<JsonConfigurationItem> GetConfigurationItems()
    {
        for (var i = 1; i <= 10; i++)
        {
            var result = new JsonConfigurationItem()
            {
                configSectionPath = $"key{i}",
                json = "{\"foo\": \"bar\"}"
            };

            //  await Task.Delay(1000);
            yield return result;
        }
    }

}
