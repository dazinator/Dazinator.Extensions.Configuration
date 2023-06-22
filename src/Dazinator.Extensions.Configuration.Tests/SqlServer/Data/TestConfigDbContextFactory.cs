namespace Dazinator.Extensions.Configuration.Tests.SqlServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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

