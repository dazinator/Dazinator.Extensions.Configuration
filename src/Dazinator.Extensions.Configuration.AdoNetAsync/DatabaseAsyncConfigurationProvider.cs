namespace Dazinator.Extensions.Configuration.AdoNetAsync;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.Async;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;


public class DatabaseAsyncConfigurationProvider : IAsyncConfigurationProvider, IDisposable
{

    private readonly DatabaseAsyncConfigurationProviderOptions _options;
    private readonly ILogger<DatabaseAsyncConfigurationProvider> _logger;
    private readonly IChangeTokenProducer? _changeTokenProducer;

    public const string ConfigSectionPathColumnName = "ConfigSectionPath";
    public const string JsonValueColumnName = "JsonValue";
    public const string TableName = "Options";


    public DatabaseAsyncConfigurationProvider(DatabaseAsyncConfigurationProviderOptions options, ILogger<DatabaseAsyncConfigurationProvider> logger)
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
                var configData = DeserializeNestedProperties(jsonValue);

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

    private Dictionary<string, string> DeserializeNestedProperties(string json)
    {
        var configData = new Dictionary<string, string>();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new NestedPropertiesConverter(configData));

        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);

        if (jsonObject != null)
        {
            foreach (var (key, value) in jsonObject)
            {
                VisitProperty(key, value, "", configData);
            }
        }

        return configData;
    }

    private void VisitProperty(string propertyName, object propertyValue, string currentPath, Dictionary<string, string> configData)
    {
        var fullPath = $"{currentPath}{propertyName}:";

        if (propertyValue is IDictionary<string, object> nestedObject)
        {
            foreach (var (key, value) in nestedObject)
            {
                VisitProperty(key, value, fullPath, configData);
            }
        }
        else
        {
            configData[fullPath] = propertyValue?.ToString();
        }
    }

    public void Dispose()
    {
        if (_changeTokenProducer is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
