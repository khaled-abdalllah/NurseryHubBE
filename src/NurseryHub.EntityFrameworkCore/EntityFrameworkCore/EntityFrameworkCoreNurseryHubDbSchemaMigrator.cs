using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NurseryHub.Data;
using Volo.Abp.DependencyInjection;

namespace NurseryHub.EntityFrameworkCore;

public class EntityFrameworkCoreNurseryHubDbSchemaMigrator
    : INurseryHubDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreNurseryHubDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the NurseryHubDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<NurseryHubDbContext>()
            .Database
            .MigrateAsync();
    }
}
