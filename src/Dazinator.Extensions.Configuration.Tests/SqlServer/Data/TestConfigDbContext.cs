namespace Dazinator.Extensions.Configuration.Tests.SqlServer.Data;
using Microsoft.EntityFrameworkCore;

public class TestConfigDbContext : DbContext
{

    public TestConfigDbContext(DbContextOptions<TestConfigDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigEntity> Configs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConfigEntity>().HasData(
                new ConfigEntity { Id = 1, ConfigSectionPath = "Test", Json = "{\"Test\":\"Test\"}" },
                new ConfigEntity { Id = 2, ConfigSectionPath = "ForDelete", Json = "{\"Test\":\"Test\"}" }
                );

        //modelBuilder.Entity<ConfigEntity>
    }
}
