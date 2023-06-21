namespace Dazinator.Extensions.Configuration.Tests;
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
        //modelBuilder.Entity<ConfigEntity>
    }
}
