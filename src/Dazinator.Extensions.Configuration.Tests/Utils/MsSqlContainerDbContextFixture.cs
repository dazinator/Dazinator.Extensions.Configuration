namespace Dazinator.Extensions.Configuration.Tests.Utils;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class MsSqlContainerDbContextFixture<TDbContext> : MsSqlContainerFixture
    where TDbContext : DbContext
{


    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        await ApplyDatabaseMigrationsAsync();
    }

    public IServiceProvider ServiceProvider { get; set; }

    private async Task ApplyDatabaseMigrationsAsync()
    {
        // ensure database migrated up todate.
        IServiceCollection services = new ServiceCollection();     
        RegisterServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // apply migrations (impacts all tenants sharing this database)
        using (var scope = ServiceProvider.CreateScope())
        {
           await scope.ServiceProvider.GetRequiredService<TDbContext>().Database.MigrateAsync();           
        }
    }


    protected virtual void RegisterServices(IServiceCollection services)
    {      
        services.AddLogging(a =>
        {
          ///  a.AddDebug();
            //    .AddXUnit(OutputHelper);
        });
        services.AddDbContext<TDbContext>((sp, builder) =>
        {

          ///  builder.UseTenantId<int?>(1); // the tenant id constant that this dbcontext will use when evaluating global query filters. 
            builder.EnableSensitiveDataLogging();

            var targetDbName = typeof(TDbContext).Name + "Tests";

            var con = GetConnectionString(targetDbName);
         

            builder.UseSqlServer(con, (builder) =>
            {
                builder.EnableRetryOnFailure();
               /// builder.UseQueryableValues();
            });
        }, ServiceLifetime.Scoped, ServiceLifetime.Singleton);     
    }

    protected string GetConnectionString(string databaseName)
    {
        var con = Container.GetConnectionString();
        var connBuilder = new SqlConnectionStringBuilder(con);
        connBuilder.InitialCatalog = databaseName;
        return connBuilder.ConnectionString;
    }

    public string GetDatabaseConnectionString()
    {
        var dbName = typeof(TDbContext).Name + "Tests";
        return GetConnectionString(dbName);
    }
}

