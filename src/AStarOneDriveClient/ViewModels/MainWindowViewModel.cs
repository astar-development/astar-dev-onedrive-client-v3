using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace AStarOneDriveClient.ViewModels;

/// <summary>
/// ViewModel for the main application window, coordinating between account management and sync tree views.
/// </summary>
public sealed class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="accountManagementViewModel">The account management view model.</param>
    /// <param name="syncTreeViewModel">The sync tree view model.</param>
    public MainWindowViewModel(
        AccountManagementViewModel accountManagementViewModel,
        SyncTreeViewModel syncTreeViewModel)
    {
        ArgumentNullException.ThrowIfNull(accountManagementViewModel);
        ArgumentNullException.ThrowIfNull(syncTreeViewModel);

        AccountManagement = accountManagementViewModel;
        SyncTree = syncTreeViewModel;

        // Wire up: When an account is selected, update the sync tree
        accountManagementViewModel
            .WhenAnyValue(x => x.SelectedAccount)
            .Select(account => account?.AccountId)
            .BindTo(syncTreeViewModel, x => x.SelectedAccountId)
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
        AccountManagement.Dispose();
        SyncTree.Dispose();
    }
}
