namespace Dazinator.Extensions.Configuration.Tests.SqlServer;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.SqlServer;
using Dazinator.Extensions.Configuration.Tests.SqlServer.Data;
using Dazinator.Extensions.Configuration.Tests.SqlServer.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Categories;

[IntegrationTest]
public partial class SqlDependencyChangeTokenProducerTests : IClassFixture<MsSqlContainerDbContextFixture<TestConfigDbContext>>
{
    private readonly MsSqlContainerDbContextFixture<TestConfigDbContext> _fixture;

    public SqlDependencyChangeTokenProducerTests(MsSqlContainerDbContextFixture<TestConfigDbContext> fixture, ITestOutputHelper output) => _fixture = fixture;

    [Theory]
    [InlineData("INSERT INTO Configs (ConfigSectionPath, Json) VALUES ('foo', 'bar')")] // new config inserted
    [InlineData("UPDATE Configs SET Json = '{}' WHERE ConfigSectionPath = 'Test'")] // existing config updated json
    [InlineData("DELETE Configs WHERE ConfigSectionPath = 'ForDelete'")] // existing config updated json
    public async Task Produce_WhenConfigsTableChanged_ChangeTokenIsSignalled(string modificationSql)
    {
        // must call SqlDependency.Start() before using SqlDependency and SqlDependency.Stop() when finished.
        var con = _fixture.GetDatabaseConnectionString();
        using var sqlDependencyLifetime = new SqlDependencyLifetime(con);
        await sqlDependencyLifetime.InitialiseAsync(enableBroker: true);


        var connection = new SqlConnection(con); // var dbProviderFactory = DbProviderFactories.GetFactory(MicrosoftSqlServerDbProviderUtils.SqlServerProviderInvariantName);
        await connection.OpenAsync();


        //var options = new DbContextOptionsBuilder<TestConfigDbContext>()
        //    .UseSqlServer(con)
        //    .Options;

        var sql = "SELECT ConfigSectionPath, RowVersion FROM [dbo].Configs";
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var producer = new SqlDependencyChangeTokenProducer(connection, command, shouldDispose: true);

        // in orderfor the dependency to work, we need to execute the command once.
        using var reader = await command.ExecuteReaderAsync();
        // reader must be closed first before we can execute another command.
        await reader.CloseAsync();

        var signaled = false;
        using var subscribed = ChangeTokenExtensions.OnChange(producer.Produce, async () => signaled = true);

        using var modificationCommand = connection.CreateCommand();
        modificationCommand.CommandText = modificationSql;
        await modificationCommand.ExecuteNonQueryAsync();

        // allow some time for notification to be received.
        await Task.Delay(1000);
        await Task.Yield();

        signaled.ShouldBeTrue();

    }
}

