namespace Dazinator.Extensions.Configuration.Async;

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

public class DelegateAsyncProviderOptions
{
    /// <summary>
    /// Supplies an <see cref="IChangeToken"/> that when signalled will cause the configuration to be reloaded.
    /// </summary>
    public Func<IChangeToken> ChangeTokenProducer { get; set; }

    /// <summary>
    /// Async function that will be called to load the configuration.
    /// </summary>
    public Func<Task<IDictionary<string, string>>> OnLoadConfigurationAsync { get; set; }


    /// <summary>
    /// Set a function that will be invoked after the configuration provider completes a reload.
    /// </summary>
    public Action OnAfterReloaded { get; set; }


}
