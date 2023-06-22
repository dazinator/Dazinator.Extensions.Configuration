namespace Dazinator.Extensions.Configuration.AdoNetAsync;
using System.Data.Common;
using Microsoft.Extensions.Primitives;

public class DatabaseAsyncConfigurationProviderOptions
{
    public Func<DbConnection> ConnectionFactory { get; set; }

    public Func<DbConnection, DbCommand>? CommandFactory { get; set; } = null;

    /// <summary>
    /// Set a function that will be invoked to get a producer of change tokens. If this is not null, the change tokens produced will be used to signal the provider to reload from the database. Leave null if the provider doesn't support reload notifications.
    /// </summary>
    public Func<IChangeTokenProducer>? GetChangeTokenProducer { get; set; } = null;

}
