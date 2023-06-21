namespace Dazinator.Extensions.Configuration.Tests.Utils;
using System.Threading.Tasks;
using Testcontainers.MsSql;

public class MsSqlContainerFixture : ContainerFixture<MsSqlContainer>
{

    public MsSqlContainerFixture() => MicrosoftSqlServerDbProviderUtils.RegisterSqlServerProviderFactory();

    protected override MsSqlContainer BuildContainer()
    {
        var container = new MsSqlBuilder()
         .WithPassword("yourStrong(!)Password")
          
         .Build();

        return container;
    }

    public override async Task InitializeAsync() => await Container.StartAsync()
          .ConfigureAwait(false);


    public override async Task DisposeAsync() => await Container.DisposeAsync()
          .ConfigureAwait(false);

    public override void Dispose()
    {

    }
}
