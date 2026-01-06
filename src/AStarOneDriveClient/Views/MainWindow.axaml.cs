using Avalonia.Controls;
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

        // Retrieve the MainWindowViewModel from DI container
        if (App.Services is not null)
        {
            DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        }
    }
}
