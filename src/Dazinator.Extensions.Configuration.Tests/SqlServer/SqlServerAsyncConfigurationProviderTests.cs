namespace Dazinator.Extensions.Configuration.Tests.SqlServer;

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Dazinator.Extensions.Configuration.AdoNet;
using Dazinator.Extensions.Configuration.SqlServer;
using Dazinator.Extensions.Configuration.Tests.SqlServer.Data;
using Dazinator.Extensions.Configuration.Tests.SqlServer.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Categories;

[IntegrationTest]
public partial class SqlServerAsyncConfigurationProviderTests : IClassFixture<MsSqlContainerDbContextFixture<TestConfigDbContext>>
{
    private readonly MsSqlContainerDbContextFixture<TestConfigDbContext> _fixture;

    public SqlServerAsyncConfigurationProviderTests(MsSqlContainerDbContextFixture<TestConfigDbContext> fixture, ITestOutputHelper output) => _fixture = fixture;


    [Fact]
    public async Task CanAddSqlServerAsyncProviderProvider()
    {



        // must call SqlDependency.Start() before using SqlDependency and SqlDependency.Stop() when finished.
        var con = _fixture.GetDatabaseConnectionString();
        using var sqlDependencyLifetime = new SqlDependencyLifetime(con);
        await sqlDependencyLifetime.InitialiseAsync(enableBroker: true);

        /// todo : #1 move all this logic to the provider
        var connection = new SqlConnection(con); // var dbProviderFactory = DbProviderFactories.GetFactory(MicrosoftSqlServerDbProviderUtils.SqlServerProviderInvariantName);
        await connection.OpenAsync();

        // set up change token producer.
        var sql = "SELECT ConfigSectionPath, Json FROM [dbo].Configs";
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var producer = new SqlDependencyChangeTokenProducer(connection, command, shouldDispose: true);

        // in orderfor the dependency to work, we need to execute the command once.
        using var reader = await command.ExecuteReaderAsync();
        // reader must be closed first before we can execute another command.
        await reader.CloseAsync();
        /// #1 end


        var logger = GetLogger<SqlServerAsyncConfigurationProvider>();

        var options = new SqlServerAsyncConfigurationProviderOptions()
        {
            ConnectionFactory = () => new SqlConnection(con),
            CommandFactory = (c) =>
            {
                var command = c.CreateCommand();
                command.CommandText = sql;
                return command;
            }
        };
        var sqlAsyncProvider = new SqlServerAsyncConfigurationProvider(options, logger, () => producer.Produce());

        var jsonLogger = GetLogger<JsonItemAsyncConfigurationProvider>();
        var jsonItemAsyncProvider = new JsonItemAsyncConfigurationProvider(sqlAsyncProvider, jsonLogger, disposeItemsProviderOnDispose: true);

        var configurationBuilder = new ConfigurationBuilder();
        var asyncSource = await configurationBuilder.AddProviderAsync(jsonItemAsyncProvider, disposeWhenConfigurationDisposed: true);

        configurationBuilder.Sources.Count.ShouldBe(1);
    }

    private ILogger<T> GetLogger<T>()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<T>>();
        return logger;
    }


    //[Theory]
    //[InlineData("INSERT INTO Configs (ConfigSectionPath, Json) VALUES ('foo', 'bar')")] // new config inserted
    //[InlineData("UPDATE Configs SET Json = '{}' WHERE ConfigSectionPath = 'Test'")] // existing config updated json
    //[InlineData("DELETE Configs WHERE ConfigSectionPath = 'ForDelete'")] // existing config updated json
    //public async Task Produce_WhenConfigsTableChanged_ChangeTokenIsSignalled(string modificationSql)
    //{






    //    //var options = new DbContextOptionsBuilder<TestConfigDbContext>()
    //    //    .UseSqlServer(con)
    //    //    .Options;

    //    var sql = "SELECT ConfigSectionPath, RowVersion FROM [dbo].Configs";
    //    using var command = connection.CreateCommand();
    //    command.CommandText = sql;
    //    using var producer = new SqlDependencyChangeTokenProducer(connection, command, shouldDispose: true);

    //    // in orderfor the dependency to work, we need to execute the command once.
    //    using var reader = await command.ExecuteReaderAsync();
    //    // reader must be closed first before we can execute another command.
    //    await reader.CloseAsync();

    //    var signaled = false;
    //    using var subscribed = ChangeTokenExtensions.OnChange(producer.Produce, async () => signaled = true);

    //    using var modificationCommand = connection.CreateCommand();
    //    modificationCommand.CommandText = modificationSql;
    //    await modificationCommand.ExecuteNonQueryAsync();

    //    // allow some time for notification to be received.
    //    await Task.Delay(1000);
    //    await Task.Yield();

    //    signaled.ShouldBeTrue();

    //}
}

