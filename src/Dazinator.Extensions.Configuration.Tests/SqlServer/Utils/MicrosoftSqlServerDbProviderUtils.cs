namespace Dazinator.Extensions.Configuration.Tests.SqlServer.Utils;
using System.Data.Common;
using Microsoft.Data.SqlClient;

public class MicrosoftSqlServerDbProviderUtils
{

    public const string SqlServerProviderInvariantName = "System.Data.SqlClient";

    public static void RegisterSqlServerProviderFactory()
    {
        if (!DbProviderFactories.TryGetFactory(SqlServerProviderInvariantName, out var factory))
        {
            DbProviderFactories.RegisterFactory(SqlServerProviderInvariantName, SqlClientFactory.Instance);
        }
    }

}
