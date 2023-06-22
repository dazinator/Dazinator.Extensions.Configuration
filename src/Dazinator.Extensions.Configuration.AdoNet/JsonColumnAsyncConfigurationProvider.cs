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

    public const string ConfigSectionPathColumnName = "ConfigSectionPath";
    public const string JsonValueColumnName = "JsonValue";
    public const string TableName = "Options";


    public JsonColumnAsyncConfigurationProvider(JsonColumnAsyncConfigurationProviderOptions options, ILogger<JsonColumnAsyncConfigurationProvider> logger)
    {
        _options = options;
        _logger = logger;
        _changeTokenProducer = options.GetChangeTokenProducer?.Invoke() ?? null;
    }

    public IChangeToken GetReloadToken() => _changeTokenProducer?.Produce() ?? EmptyChangeToken.Instance;

    public async Task<IDictionary<string, string>> LoadAsync()
    {
        _logger.LogDebug("Loading configuration from database.");
        await using var connection = _options.ConnectionFactory();
        await connection.OpenAsync();
        _logger.LogTrace("Opened connection to database.");

        using (var command = _options?.CommandFactory?.Invoke(connection) ?? connection.CreateCommand())
        {
            if (string.IsNullOrWhiteSpace(command.CommandText))
            {
                command.CommandText = $"SELECT {ConfigSectionPathColumnName}, {JsonValueColumnName} FROM {TableName}";
            }

            var result = await LoadAsync(command);
            return result;
        }
    }

    private async Task<IDictionary<string, string>> LoadAsync(DbCommand command, CancellationToken cancellation = default)
    {
        _logger.LogDebug("Executing command to load configuration from database {commandText}", command.CommandText);
        await using (var reader = await command.ExecuteReaderAsync(cancellation))
        {
            // read all rows into a dictionary of config keys and values asynchronously.
            var data = new Dictionary<string, string>();
            //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            //options.Converters.Add(new NestedPropertiesConverter(configData));

            while (await reader.ReadAsync(cancellation))
            {
                var configSectionPath = reader.GetString(0); // e.g MySection:MySubSection or to support named options binding MySection:MySubSection-[name]
                _logger.LogTrace("Loading configuration for section {configSectionPath}", configSectionPath);

                var jsonValue = reader.GetString(1);
                var configData = LoadConfiguration(jsonValue);

                if (configData != null)
                {
                    foreach (var (key, value) in configData)
                    {
                        var fullConfigKey = $"{configSectionPath}:{key}";
                        _logger.LogTrace("Loading configuration for key {fullConfigKey}", fullConfigKey);
                        data[fullConfigKey] = value;
                    }
                }
            }

            _logger.LogTrace("Finished loading configuration from database.");
            return data;

        }
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
