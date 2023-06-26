namespace Dazinator.Extensions.Configuration.AdoNet;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.Async;
using Dazinator.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

/// <summary>
/// An <see cref="IAsyncConfigurationProvider"/> that maps onfiguration from a list of <see cref="JsonConfigurationItem"/>s."/>
/// </summary>
public class JsonItemAsyncConfigurationProvider : IAsyncConfigurationProvider, IDisposable
{

    private readonly JsonColumnAsyncConfigurationProviderOptions _options;
    private readonly ILogger<JsonItemAsyncConfigurationProvider> _logger;
    private readonly IChangeTokenProducer? _changeTokenProducer;


    public JsonItemAsyncConfigurationProvider(JsonColumnAsyncConfigurationProviderOptions options, ILogger<JsonItemAsyncConfigurationProvider> logger)
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
        if (conf != null)
        {
            // read all rows into a dictionary of config keys and values asynchronously.
            var items = await conf;

            foreach (var item in items)
            {
                var configSectionPath = item.configSectionPath; // e.g MySection:MySubSection or to support named options binding MySection:MySubSection-[name]
                _logger.LogTrace("Loading configuration for section {configSectionPath}", configSectionPath);

                // todo: if we had a rowversion we could compare and only if its changed bother to load the json otherwise we could keep the existing config for this section?
                // might be complicated consider two seperate records "foo:bar" and "foo:bar:baz" - if we are currently processing "foo:bar" and it hasn't' changed, we can't assume all keys with "foo:bar" prefix haven't changed because "foo:bar:baz" might have changed.
                // so this implies some sort order or further complication to the processing. For not just easier to process all configuration from scratch on each reload.
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
