using Avalonia.Controls;
using AStarOneDriveClient.Authentication;
using AStarOneDriveClient.Repositories;
using AStarOneDriveClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AStarOneDriveClient.Views;

/// <summary>
/// Main application window.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Retrieve dependencies from DI container
        if (App.Services is not null)
        {
            var authService = App.Services.GetRequiredService<IAuthService>();
            var accountRepository = App.Services.GetRequiredService<IAccountRepository>();

            // Set the DataContext for the entire window
            DataContext = new AccountManagementViewModel(authService, accountRepository);
        }
    }
}
