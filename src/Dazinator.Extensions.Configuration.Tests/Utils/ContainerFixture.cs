namespace Dazinator.Extensions.Configuration.Tests.Utils;
using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;

/// <summary>
/// A Testcontainers database class fixture.
/// </summary>
/// <typeparam name="TDockerContainer">Type of <see cref="ITestcontainersContainer" />.</typeparam>  
#pragma warning disable SA1649

public abstract class ContainerFixture<TDockerContainer> : IAsyncLifetime, IDisposable
  where TDockerContainer : IContainer

#pragma warning restore SA1649
{

    public ContainerFixture() => Container = BuildContainer();

    protected abstract TDockerContainer BuildContainer();

    /// <summary>
    /// Gets or sets the Testcontainers.
    /// </summary>
    public TDockerContainer Container { get; protected set; }


    /// <inheritdoc />
    public abstract Task InitializeAsync();

    /// <inheritdoc />
    public abstract Task DisposeAsync();

    /// <inheritdoc />
    public abstract void Dispose();
}
