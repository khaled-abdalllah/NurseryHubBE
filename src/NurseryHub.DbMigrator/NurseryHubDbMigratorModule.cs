using NurseryHub.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace NurseryHub.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(NurseryHubEntityFrameworkCoreModule),
    typeof(NurseryHubApplicationContractsModule),
    typeof(NurseryHubApplicationModule)
)]
public class NurseryHubDbMigratorModule : AbpModule
{
}
