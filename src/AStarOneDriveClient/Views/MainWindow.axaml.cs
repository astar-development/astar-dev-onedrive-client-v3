using Avalonia;
using Avalonia.Controls;
using AStarOneDriveClient.Models;
using AStarOneDriveClient.Services;
using AStarOneDriveClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AStarOneDriveClient.Views;

/// <summary>
/// Main application window.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IWindowPreferencesService? _preferencesService;

    public MainWindow()
    {
        InitializeComponent();

        // Retrieve the MainWindowViewModel from DI container
        if (App.Services is not null)
        {
            DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
            _preferencesService = App.Services.GetService<IWindowPreferencesService>();

            // Load and apply saved window position
            _ = LoadWindowPreferencesAsync();
        }

        // Save window position when it changes
        PositionChanged += OnPositionChanged;
        PropertyChanged += OnWindowPropertyChanged;
    }

    private async System.Threading.Tasks.Task LoadWindowPreferencesAsync()
    {
        if (_preferencesService is null)
            return;

        try
        {
            WindowPreferences? preferences = await _preferencesService.LoadAsync();
            if (preferences is not null)
            {
                // Apply saved preferences
                if (preferences.IsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
                else if (preferences.X.HasValue && preferences.Y.HasValue)
                {
                    Position = new PixelPoint((int)preferences.X.Value, (int)preferences.Y.Value);
                    Width = preferences.Width;
                    Height = preferences.Height;
                }
            }
        }
        catch
        {
            // Ignore errors loading preferences - use defaults
        }
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        _ = SaveWindowPreferencesAsync();
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty || e.Property == WidthProperty || e.Property == HeightProperty)
        {
            _ = SaveWindowPreferencesAsync();
        }
    }

    private async System.Threading.Tasks.Task SaveWindowPreferencesAsync()
    {
        if (_preferencesService is null)
            return;

        try
        {
            var preferences = new WindowPreferences(
                Id: 1,
                X: WindowState == WindowState.Normal ? Position.X : null,
                Y: WindowState == WindowState.Normal ? Position.Y : null,
                Width: Width,
                Height: Height,
                IsMaximized: WindowState == WindowState.Maximized
            );

            await _preferencesService.SaveAsync(preferences);
        }
        catch
        {
            // Ignore errors saving preferences
        }
    }
}
