using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NurseryHub.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class NurseryHubDbContextFactory : IDesignTimeDbContextFactory<NurseryHubDbContext>
{
    public NurseryHubDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        NurseryHubEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<NurseryHubDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new NurseryHubDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../NurseryHub.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
