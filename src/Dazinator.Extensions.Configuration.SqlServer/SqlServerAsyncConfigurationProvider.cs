namespace Dazinator.Extensions.Configuration.SqlServer;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.AdoNet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

public class SqlServerAsyncConfigurationProvider : IAsyncItemProvider<IList<JsonConfigurationItem>>
{
    private readonly SqlServerAsyncConfigurationProviderOptions _options;
    private readonly ILogger<SqlServerAsyncConfigurationProvider> _logger;
    private readonly Func<IChangeToken> _changeTokenProducer;

    public const string ConfigSectionPathColumnName = "ConfigSectionPath";
    public const string JsonValueColumnName = "JsonValue";
    public const string TableName = "Options";

    public SqlServerAsyncConfigurationProvider(
        SqlServerAsyncConfigurationProviderOptions options,
        ILogger<SqlServerAsyncConfigurationProvider> logger,
        Func<IChangeToken> changeTokenProducer = null)
    {
        _options = options;
        _logger = logger;
        _changeTokenProducer = changeTokenProducer;
    }

    public IChangeToken GetReloadToken() => _changeTokenProducer.Invoke();
    public async Task<IList<JsonConfigurationItem>> LoadAsync()
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

    private async Task<IList<JsonConfigurationItem>> LoadAsync(DbCommand command, CancellationToken cancellation = default)
    {

        var results = new List<JsonConfigurationItem>();

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
                var item = new JsonConfigurationItem(configSectionPath, jsonValue);
                results.Add(item);
                _logger.LogTrace("Finished loading configuration from database.");
            }
        }

        return results;
    }
}
