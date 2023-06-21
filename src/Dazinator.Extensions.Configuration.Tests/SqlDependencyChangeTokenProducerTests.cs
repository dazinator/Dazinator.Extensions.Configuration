namespace Dazinator.Extensions.Configuration.Tests;
using System.Threading.Tasks;
using Dazinator.Extensions.Configuration.SqlServer;
using Dazinator.Extensions.Configuration.Tests.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Categories;

[IntegrationTest]
public partial class SqlDependencyChangeTokenProducerTests : IClassFixture<MsSqlContainerDbContextFixture<TestConfigDbContext>>
{
    private readonly MsSqlContainerDbContextFixture<TestConfigDbContext> _fixture;

    public SqlDependencyChangeTokenProducerTests(MsSqlContainerDbContextFixture<TestConfigDbContext> fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Produce_WhenTableChanged_ChangeTokenIsSignalled()
    {
        var con = _fixture.GetDatabaseConnectionString();
        using var sqlDependencyLifetime = new SqlDependencyLifetime(con);
        await sqlDependencyLifetime.InitialiseAsync(enableBroker: true);

    

        var connection = new SqlConnection(con); // var dbProviderFactory = DbProviderFactories.GetFactory(MicrosoftSqlServerDbProviderUtils.SqlServerProviderInvariantName);
        await connection.OpenAsync();    
      

        //var options = new DbContextOptionsBuilder<TestConfigDbContext>()
        //    .UseSqlServer(con)
        //    .Options;

        var sql = "SELECT ConfigSectionPath, Json FROM Configs";
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var producer = new SqlDependencyChangeTokenProducer(connection, command, shouldDispose: true);

        // in orderfor the dependency to work, we need to execute the command once.
        using var reader = await command.ExecuteReaderAsync();


        //// await connection.OpenAsync();

        bool signaled = false;
        using var subscribed = ChangeTokenExtensions.OnChange(producer.Produce, async () =>
        {
            signaled = true;
        });

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = "INSERT INTO Configs (ConfigSectionPath, Json) VALUES ('foo', 'bar')";
        await insertCommand.ExecuteNonQueryAsync();

        await Task.Delay(5000);
        await Task.Yield();
        await Task.Delay(5000);

        signaled.ShouldBeTrue();

    }   
}

//public class TestConfigDbContextFactory : IDesignTimeDbContextFactory<TestConfigDbContext>
//{
//    public TestConfigDbContext CreateDbContext(string[] args)
//    {

//        var container = new MsSqlBuilder()
//       .WithPassword("yourStrong(!)Password")
//       .Build();

//        var optionsBuilder = new DbContextOptionsBuilder<TestConfigDbContext>();
//        optionsBuilder.UseSqlServer(container.GetConnectionString());
//        return new TestConfigDbContext(optionsBuilder.Options);
//    }
//}

public class TestConfigDbContextFactory : IDesignTimeDbContextFactory<TestConfigDbContext>
{
    public const string DesignTimeConnString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ConfigTests;Integrated Security=SSPI;";
    public TestConfigDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestConfigDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConnString);
        return new TestConfigDbContext(optionsBuilder.Options);
    }
}

