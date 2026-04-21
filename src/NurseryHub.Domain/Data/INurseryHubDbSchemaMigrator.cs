using System.Threading.Tasks;

namespace NurseryHub.Data;

public interface INurseryHubDbSchemaMigrator
{
    Task MigrateAsync();
}
