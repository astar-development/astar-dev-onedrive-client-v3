using AStarOneDriveClient.Repositories;
using AStarOneDriveClient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AStarOneDriveClient.Tests.Unit.Integration;

public class ServiceConfigurationShould
{
    [Fact]
    public void ConfigureAllServicesCorrectly()
    {
        var serviceProvider = ServiceConfiguration.ConfigureServices();

        serviceProvider.ShouldNotBeNull();
    }

    [Fact]
    public void ResolveAccountRepositorySuccessfully()
    {
        using var serviceProvider = ServiceConfiguration.ConfigureServices();

        var repository = serviceProvider.GetService<IAccountRepository>();

        repository.ShouldNotBeNull();
        repository.ShouldBeOfType<AccountRepository>();
    }

    [Fact]
    public void ResolveSyncConfigurationRepositorySuccessfully()
    {
        using var serviceProvider = ServiceConfiguration.ConfigureServices();

        var repository = serviceProvider.GetService<ISyncConfigurationRepository>();

        repository.ShouldNotBeNull();
        repository.ShouldBeOfType<SyncConfigurationRepository>();
    }

    [Fact]
    public void ResolveFileMetadataRepositorySuccessfully()
    {
        using var serviceProvider = ServiceConfiguration.ConfigureServices();

        var repository = serviceProvider.GetService<IFileMetadataRepository>();

        repository.ShouldNotBeNull();
        repository.ShouldBeOfType<FileMetadataRepository>();
    }

    [Fact]
    public void ResolveWindowPreferencesServiceSuccessfully()
    {
        using var serviceProvider = ServiceConfiguration.ConfigureServices();

        var service = serviceProvider.GetService<IWindowPreferencesService>();

        service.ShouldNotBeNull();
        service.ShouldBeOfType<WindowPreferencesService>();
    }

    [Fact]
    public void CreateScopedInstancesForRepositories()
    {
        using var serviceProvider = ServiceConfiguration.ConfigureServices();

        IAccountRepository? repo1;
        IAccountRepository? repo2;

        using (var scope1 = serviceProvider.CreateScope())
        {
            repo1 = scope1.ServiceProvider.GetService<IAccountRepository>();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            repo2 = scope2.ServiceProvider.GetService<IAccountRepository>();
        }

        repo1.ShouldNotBeNull();
        repo2.ShouldNotBeNull();
        repo1.ShouldNotBeSameAs(repo2);
    }

    [Fact]
    public void EnsureDatabaseCreatedDoesNotThrow()
    {
        using var serviceProvider = ServiceConfiguration.ConfigureServices();

        Should.NotThrow(() => ServiceConfiguration.EnsureDatabaseCreated(serviceProvider));
    }
}
