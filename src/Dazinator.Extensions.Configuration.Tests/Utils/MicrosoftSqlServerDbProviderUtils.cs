namespace Dazinator.Extensions.Configuration.Tests.Utils;
using System.Data.Common;
using Microsoft.Data.SqlClient;

public class MicrosoftSqlServerDbProviderUtils
{

    public const string SqlServerProviderInvariantName = "System.Data.SqlClient";

    public static void RegisterSqlServerProviderFactory()
    {
        if (!DbProviderFactories.TryGetFactory(SqlServerProviderInvariantName, out DbProviderFactory factory))
        {
            DbProviderFactories.RegisterFactory(SqlServerProviderInvariantName, SqlClientFactory.Instance);
        }
    }

}
