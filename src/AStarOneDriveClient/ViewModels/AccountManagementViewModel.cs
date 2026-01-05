using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AStarOneDriveClient.Models;
using ReactiveUI;

namespace AStarOneDriveClient.ViewModels;

/// <summary>
/// ViewModel for managing OneDrive accounts.
/// </summary>
public sealed class AccountManagementViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private AccountInfo? _selectedAccount;
    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountManagementViewModel"/> class.
    /// </summary>
    public AccountManagementViewModel()
    {
        Accounts = new ObservableCollection<AccountInfo>();

        // Add Account command - always enabled
        AddAccountCommand = ReactiveCommand.Create(
            () => { },
            outputScheduler: RxApp.MainThreadScheduler);

        // Remove Account command - enabled when account is selected
        var canRemove = this.WhenAnyValue(x => x.SelectedAccount)
            .Select(account => account is not null);
        RemoveAccountCommand = ReactiveCommand.Create(
            () => { },
            canRemove,
            RxApp.MainThreadScheduler);

        // Login command - enabled when account is selected and not authenticated
        var canLogin = this.WhenAnyValue(x => x.SelectedAccount)
            .Select(account => account is not null && !account.IsAuthenticated);
        LoginCommand = ReactiveCommand.CreateFromTask(
            async () => await Task.CompletedTask,
            canLogin,
            RxApp.MainThreadScheduler);

        // Logout command - enabled when account is selected and authenticated
        var canLogout = this.WhenAnyValue(x => x.SelectedAccount)
            .Select(account => account is not null && account.IsAuthenticated);
        LogoutCommand = ReactiveCommand.CreateFromTask(
            async () => await Task.CompletedTask,
            canLogout,
            RxApp.MainThreadScheduler);

        // Dispose observables
        canRemove.Subscribe().DisposeWith(_disposables);
        canLogin.Subscribe().DisposeWith(_disposables);
        canLogout.Subscribe().DisposeWith(_disposables);
    }

    /// <summary>
    /// Gets the collection of accounts.
    /// </summary>
    public ObservableCollection<AccountInfo> Accounts { get; }

    /// <summary>
    /// Gets or sets the currently selected account.
    /// </summary>
    public AccountInfo? SelectedAccount
    {
        get => _selectedAccount;
        set => this.RaiseAndSetIfChanged(ref _selectedAccount, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view is loading data.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    /// <summary>
    /// Gets the command to add a new account.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddAccountCommand { get; }

    /// <summary>
    /// Gets the command to remove the selected account.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemoveAccountCommand { get; }

    /// <summary>
    /// Gets the command to login to the selected account.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    /// <summary>
    /// Gets the command to logout from the selected account.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
