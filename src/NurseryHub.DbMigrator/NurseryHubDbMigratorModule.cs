using NurseryHub.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace NurseryHub.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(NurseryHubEntityFrameworkCoreModule),
    typeof(NurseryHubApplicationContractsModule)
)]
public class NurseryHubDbMigratorModule : AbpModule
{
}
