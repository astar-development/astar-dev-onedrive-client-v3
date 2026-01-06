using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace AStarOneDriveClient.ViewModels;

/// <summary>
/// ViewModel for the main application window, coordinating between account management and sync tree views.
/// </summary>
public sealed class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly IServiceProvider _serviceProvider;

    private SyncProgressViewModel? _syncProgress;
    /// <summary>
    /// Gets the sync progress view model (when sync is active).
    /// </summary>
    public SyncProgressViewModel? SyncProgress
    {
        get => _syncProgress;
        private set => this.RaiseAndSetIfChanged(ref _syncProgress, value);
    }

    /// <summary>
    /// Gets a value indicating whether sync progress view should be shown.
    /// </summary>
    public bool ShowSyncProgress => SyncProgress is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="accountManagementViewModel">The account management view model.</param>
    /// <param name="syncTreeViewModel">The sync tree view model.</param>
    /// <param name="serviceProvider">The service provider for creating SyncProgressViewModel instances.</param>
    public MainWindowViewModel(
        AccountManagementViewModel accountManagementViewModel,
        SyncTreeViewModel syncTreeViewModel,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(accountManagementViewModel);
        ArgumentNullException.ThrowIfNull(syncTreeViewModel);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        AccountManagement = accountManagementViewModel;
        SyncTree = syncTreeViewModel;
        _serviceProvider = serviceProvider;

        // Wire up: When an account is selected, update the sync tree
        accountManagementViewModel
            .WhenAnyValue(x => x.SelectedAccount)
            .Select(account => account?.AccountId)
            .BindTo(syncTreeViewModel, x => x.SelectedAccountId)
            .DisposeWith(_disposables);

        // Wire up: When sync starts, show sync progress view
        syncTreeViewModel
            .WhenAnyValue(x => x.IsSyncing, x => x.SelectedAccountId,
                (isSyncing, accountId) => new { IsSyncing = isSyncing, AccountId = accountId })
            .Subscribe(state =>
            {
                if (state.IsSyncing && !string.IsNullOrEmpty(state.AccountId))
                {
                    // Create sync progress view model if not already created
                    if (SyncProgress is null || SyncProgress.AccountId != state.AccountId)
                    {
                        SyncProgress?.Dispose();
                        SyncProgress = ActivatorUtilities.CreateInstance<SyncProgressViewModel>(
                            _serviceProvider,
                            state.AccountId);
                    }
                }
                else if (!state.IsSyncing && SyncProgress is not null)
                {
                    // Keep showing progress view even when not syncing (shows completion/pause state)
                    // User can manually close by navigating away
                }

                this.RaisePropertyChanged(nameof(ShowSyncProgress));
            })
            .DisposeWith(_disposables);
    }

    /// <summary>
    /// Gets the account management view model.
    /// </summary>
    public AccountManagementViewModel AccountManagement { get; }

    /// <summary>
    /// Gets the sync tree view model.
    /// </summary>
    public SyncTreeViewModel SyncTree { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposables.Dispose();
        SyncProgress?.Dispose();
        AccountManagement.Dispose();
        SyncTree.Dispose();
    }
}
