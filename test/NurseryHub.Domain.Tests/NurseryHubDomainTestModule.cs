using Volo.Abp.Modularity;

namespace NurseryHub;

[DependsOn(
    typeof(NurseryHubDomainModule),
    typeof(NurseryHubTestBaseModule)
)]
public class NurseryHubDomainTestModule : AbpModule
{

}
