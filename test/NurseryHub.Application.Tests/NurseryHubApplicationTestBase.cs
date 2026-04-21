using Volo.Abp.Modularity;

namespace NurseryHub;

public abstract class NurseryHubApplicationTestBase<TStartupModule> : NurseryHubTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
