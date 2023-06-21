namespace Dazinator.Extensions.Configuration.SqlServer;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


public class SqlDependencyLifetime : IDisposable
{
    private readonly string _connectionString;

    public SqlDependencyLifetime(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitialiseAsync(bool enableBroker)
    {
        if (enableBroker)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var enableBrokerCommand = connection.CreateCommand())
                {
                    enableBrokerCommand.CommandText = "ALTER DATABASE CURRENT SET ENABLE_BROKER WITH rollback immediate;";
                    await enableBrokerCommand.ExecuteNonQueryAsync();
                }
            }
        }

        SqlDependency.Start(_connectionString);
    }
    public void Dispose()
    {
        SqlDependency.Stop(_connectionString);
    }
}

