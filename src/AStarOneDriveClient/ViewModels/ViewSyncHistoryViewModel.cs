using System.Collections.ObjectModel;
using System.Reactive;
using AStarOneDriveClient.Models;
using AStarOneDriveClient.Repositories;
using ReactiveUI;

namespace AStarOneDriveClient.ViewModels;

/// <summary>
/// ViewModel for the View Sync History window.
/// </summary>
public sealed class ViewSyncHistoryViewModel : ReactiveObject
{
    private readonly IAccountRepository _accountRepository;
    private readonly IFileOperationLogRepository _fileOperationLogRepository;
    private AccountInfo? _selectedAccount;
    private int _currentPage = 1;
    private bool _hasMoreRecords = true;
    private bool _isLoading;

    private const int PageSize = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewSyncHistoryViewModel"/> class.
    /// </summary>
    /// <param name="accountRepository">Repository for account data.</param>
    /// <param name="fileOperationLogRepository">Repository for file operation logs.</param>
    public ViewSyncHistoryViewModel(
        IAccountRepository accountRepository,
        IFileOperationLogRepository fileOperationLogRepository)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(fileOperationLogRepository);

        _accountRepository = accountRepository;
        _fileOperationLogRepository = fileOperationLogRepository;

        Accounts = new ObservableCollection<AccountInfo>();
        SyncHistory = new ObservableCollection<FileOperationLog>();

        LoadNextPageCommand = ReactiveCommand.CreateFromTask(LoadNextPageAsync);
        LoadPreviousPageCommand = ReactiveCommand.CreateFromTask(LoadPreviousPageAsync);

        // Load accounts on initialization
        _ = LoadAccountsAsync();
    }

    /// <summary>
    /// Gets the collection of accounts.
    /// </summary>
    public ObservableCollection<AccountInfo> Accounts { get; }

    /// <summary>
    /// Gets the collection of sync history records.
    /// </summary>
    public ObservableCollection<FileOperationLog> SyncHistory { get; }

    /// <summary>
    /// Gets or sets the selected account.
    /// </summary>
    public AccountInfo? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAccount, value);
            if (value is not null)
            {
                _currentPage = 1;
                _ = LoadSyncHistoryAsync();
            }
        }
    }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    /// <summary>
    /// Gets a value indicating whether there are more records to load.
    /// </summary>
    public bool HasMoreRecords
    {
        get => _hasMoreRecords;
        private set => this.RaiseAndSetIfChanged(ref _hasMoreRecords, value);
    }

    /// <summary>
    /// Gets a value indicating whether data is currently loading.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    /// <summary>
    /// Gets a value indicating whether the previous page button should be enabled.
    /// </summary>
    public bool CanGoToPreviousPage => CurrentPage > 1 && !IsLoading;

    /// <summary>
    /// Gets a value indicating whether the next page button should be enabled.
    /// </summary>
    public bool CanGoToNextPage => HasMoreRecords && !IsLoading;

    /// <summary>
    /// Gets the command to load the next page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadNextPageCommand { get; }

    /// <summary>
    /// Gets the command to load the previous page.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadPreviousPageCommand { get; }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var accounts = await _accountRepository.GetAllAsync();
            Accounts.Clear();
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }
        }
        catch
        {
            // Silently fail - user can retry by reopening window
        }
    }

    private async Task LoadSyncHistoryAsync()
    {
        if (SelectedAccount is null)
        {
            SyncHistory.Clear();
            return;
        }

        IsLoading = true;
        try
        {
            var skip = (CurrentPage - 1) * PageSize;
            var records = await _fileOperationLogRepository.GetByAccountIdAsync(
                SelectedAccount.AccountId,
                PageSize + 1, // Fetch one extra to determine if more records exist
                skip);

            SyncHistory.Clear();
            HasMoreRecords = records.Count > PageSize;

            // Only add PageSize records (exclude the extra one used for hasMore check)
            var recordsToShow = HasMoreRecords ? records.Take(PageSize) : records;
            foreach (var record in recordsToShow)
            {
                SyncHistory.Add(record);
            }

            this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
            this.RaisePropertyChanged(nameof(CanGoToNextPage));
        }
        catch
        {
            // Silently fail - display will remain empty
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadNextPageAsync()
    {
        if (!HasMoreRecords || SelectedAccount is null)
        {
            return;
        }

        CurrentPage++;
        await LoadSyncHistoryAsync();
    }

    private async Task LoadPreviousPageAsync()
    {
        if (CurrentPage <= 1 || SelectedAccount is null)
        {
            return;
        }

        CurrentPage--;
        await LoadSyncHistoryAsync();
    }
}
