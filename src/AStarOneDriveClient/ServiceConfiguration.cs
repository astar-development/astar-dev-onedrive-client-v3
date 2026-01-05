using AStarOneDriveClient.Data;
using AStarOneDriveClient.Repositories;
using AStarOneDriveClient.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AStarOneDriveClient;

/// <summary>
/// Configures dependency injection services for the application.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures and returns the service provider with all application services.
    /// </summary>
    /// <returns>Configured service provider.</returns>
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Database
        services.AddDbContext<SyncDbContext>(options =>
            options.UseSqlite(DatabaseConfiguration.ConnectionString));

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ISyncConfigurationRepository, SyncConfigurationRepository>();
        services.AddScoped<ISyncStateRepository, SyncStateRepository>();
        services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();

        // Services
        services.AddScoped<IWindowPreferencesService, WindowPreferencesService>();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Ensures the database is created and migrations are applied.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void EnsureDatabaseCreated(ServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
        context.Database.EnsureCreated();
    }
}
