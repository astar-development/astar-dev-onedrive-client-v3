using AStar.Dev.OneDrive.Client.Core;
using AStar.Dev.OneDrive.Client.Infrastructure.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AStar.Dev.OneDrive.Client;

/// <summary>
///     Main application class for the OneDrive sync client.
/// </summary>
public sealed class App : Application
{
    /// <summary>
    ///     Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }
    
    public static IHost Host { get; private set; } = null!;
    
    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        Host = AppHost.BuildHost();
        Services = Host.Services;

        _ = Host.StartAsync();
        AppHost.EnsureDatabaseUpdated(Services!);

        // Start auto-sync scheduler
        IAutoSyncSchedulerService scheduler = Services!.GetRequiredService<IAutoSyncSchedulerService>();
        _ = scheduler.StartAsync();

        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow.MainWindow();

            desktop.Startup += async (_, _) => await DebugLog.InfoAsync("App Startup", "Application has started", AdminAccountMetadata.AccountId, CancellationToken.None);

            desktop.Exit += (_, _) => scheduler.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
