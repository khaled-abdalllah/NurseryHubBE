using Volo.Abp.Modularity;

namespace NurseryHub;

[DependsOn(
    typeof(NurseryHubApplicationModule),
    typeof(NurseryHubDomainTestModule)
)]
public class NurseryHubApplicationTestModule : AbpModule
{

}
