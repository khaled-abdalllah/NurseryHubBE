using Volo.Abp.Modularity;

namespace NurseryHub;

/* Inherit from this class for your domain layer tests. */
public abstract class NurseryHubDomainTestBase<TStartupModule> : NurseryHubTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
