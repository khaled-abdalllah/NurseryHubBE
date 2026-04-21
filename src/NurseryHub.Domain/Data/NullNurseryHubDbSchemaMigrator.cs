using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace NurseryHub.Data;

/* This is used if database provider does't define
 * INurseryHubDbSchemaMigrator implementation.
 */
public class NullNurseryHubDbSchemaMigrator : INurseryHubDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
