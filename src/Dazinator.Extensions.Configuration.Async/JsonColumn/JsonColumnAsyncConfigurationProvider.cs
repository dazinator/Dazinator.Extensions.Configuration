namespace Dazinator.Extensions.Configuration.AdoNet;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.Async;
using Dazinator.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

/// <summary>
/// An <see cref="IAsyncConfigurationProvider"/> that supplies configuration from a <see cref="DbCommand"/> that returns a result set where each row has a column for the config section path, and a column for JSON object representing the configuration that will be mapped at that path."/>
/// </summary>
public class JsonColumnAsyncConfigurationProvider : IAsyncConfigurationProvider, IDisposable
{

    private readonly JsonColumnAsyncConfigurationProviderOptions _options;
    private readonly ILogger<JsonColumnAsyncConfigurationProvider> _logger;
    private readonly IChangeTokenProducer? _changeTokenProducer;


    public JsonColumnAsyncConfigurationProvider(JsonColumnAsyncConfigurationProviderOptions options, ILogger<JsonColumnAsyncConfigurationProvider> logger)
    {
        _options = options;
        _logger = logger;
        _changeTokenProducer = options.GetChangeTokenProducer?.Invoke() ?? null;
    }

    public IChangeToken GetReloadToken() => _changeTokenProducer?.Produce() ?? EmptyChangeToken.Instance;

    public async Task<IDictionary<string, string>> LoadAsync()
    {
        _logger.LogDebug("Loading configuration.");
        var data = new Dictionary<string, string>();
        var conf = _options.GetConfigurationItems?.Invoke();
        if(conf != null)
        {
            // read all rows into a dictionary of config keys and values asynchronously.           
            await foreach (var item in conf)
            {
                var configSectionPath = item.configSectionPath; // e.g MySection:MySubSection or to support named options binding MySection:MySubSection-[name]
                _logger.LogTrace("Loading configuration for section {configSectionPath}", configSectionPath);

                var jsonValue = item.json;
                var configData = LoadConfiguration(jsonValue);

                if (configData != null)
                {
                    foreach (var (key, value) in configData)
                    {
                        var fullConfigKey = $"{configSectionPath}:{key}";
                        _logger.LogTrace("Setting configuration for key {fullConfigKey}", fullConfigKey);
                        data[fullConfigKey] = value;
                    }
                }
            }
        }
        return data;
     
    }    

    private IDictionary<string, string?> LoadConfiguration(string json) => JsonConfigurationParser.Parse(json);

    public void Dispose()
    {
        if (_changeTokenProducer is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
