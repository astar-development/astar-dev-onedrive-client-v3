using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using AStarOneDriveClient.Models;
using AStarOneDriveClient.Services;
using AStarOneDriveClient.Services.OneDriveServices;
using ReactiveUI;

namespace AStarOneDriveClient.ViewModels;

/// <summary>
/// ViewModel for the sync tree view, managing folder hierarchy and selection.
/// </summary>
public sealed class SyncTreeViewModel : ReactiveObject, IDisposable
{
    private readonly IFolderTreeService _folderTreeService;
    private readonly ISyncSelectionService _selectionService;
    private readonly CompositeDisposable _disposables = new();

    private string? _selectedAccountId;
    /// <summary>
    /// Gets or sets the currently selected account ID.
    /// </summary>
    public string? SelectedAccountId
    {
        get => _selectedAccountId;
        set => this.RaiseAndSetIfChanged(ref _selectedAccountId, value);
    }

    private ObservableCollection<OneDriveFolderNode> _rootFolders = [];
    /// <summary>
    /// Gets the root-level folders for the selected account.
    /// </summary>
    public ObservableCollection<OneDriveFolderNode> RootFolders
    {
        get => _rootFolders;
        private set => this.RaiseAndSetIfChanged(ref _rootFolders, value);
    }

    private bool _isLoading;
    /// <summary>
    /// Gets a value indicating whether folders are currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private string? _errorMessage;
    /// <summary>
    /// Gets the error message if loading fails.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// Command to load root folders for the selected account.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadFoldersCommand { get; }

    /// <summary>
    /// Command to load child folders when a node is expanded.
    /// </summary>
    public ReactiveCommand<OneDriveFolderNode, Unit> LoadChildrenCommand { get; }

    /// <summary>
    /// Command to toggle folder selection state.
    /// </summary>
    public ReactiveCommand<OneDriveFolderNode, Unit> ToggleSelectionCommand { get; }

    /// <summary>
    /// Command to clear all selections.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClearSelectionsCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncTreeViewModel"/> class.
    /// </summary>
    /// <param name="folderTreeService">Service for loading folder hierarchies.</param>
    /// <param name="selectionService">Service for managing selection state.</param>
    public SyncTreeViewModel(IFolderTreeService folderTreeService, ISyncSelectionService selectionService)
    {
        _folderTreeService = folderTreeService ?? throw new ArgumentNullException(nameof(folderTreeService));
        _selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));

        LoadFoldersCommand = ReactiveCommand.CreateFromTask(LoadFoldersAsync);
        LoadChildrenCommand = ReactiveCommand.CreateFromTask<OneDriveFolderNode>(LoadChildrenAsync);
        ToggleSelectionCommand = ReactiveCommand.Create<OneDriveFolderNode>(ToggleSelection);
        ClearSelectionsCommand = ReactiveCommand.Create(ClearSelections);

        this.WhenAnyValue(x => x.SelectedAccountId)
            .Subscribe(_ => LoadFoldersCommand.Execute().Subscribe())
            .DisposeWith(_disposables);
    }

    /// <summary>
    /// Loads root folders for the currently selected account.
    /// </summary>
    private async Task LoadFoldersAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(SelectedAccountId))
        {
            RootFolders.Clear();
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var folders = await _folderTreeService.GetRootFoldersAsync(SelectedAccountId, cancellationToken);

            RootFolders.Clear();
            foreach (var folder in folders)
            {
                RootFolders.Add(folder);
            }

            // Load saved selections from database
            await _selectionService.LoadSelectionsFromDatabaseAsync(SelectedAccountId, [.. RootFolders], cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load folders: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads child folders for a specific folder node.
    /// </summary>
    /// <param name="folder">The folder whose children should be loaded.</param>
    private async Task LoadChildrenAsync(OneDriveFolderNode folder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folder);

        if (folder.ChildrenLoaded || string.IsNullOrEmpty(SelectedAccountId))
            return;

        try
        {
            var children = await _folderTreeService.GetChildFoldersAsync(SelectedAccountId, folder.Id, cancellationToken);

            folder.Children.Clear();
            foreach (var child in children)
            {
                // Inherit parent's selection state for new children
                if (folder.SelectionState == SelectionState.Checked)
                {
                    child.SelectionState = SelectionState.Checked;
                    child.IsSelected = true;
                }

                folder.Children.Add(child);
            }

            folder.ChildrenLoaded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load child folders: {ex.Message}";
        }
    }

    /// <summary>
    /// Toggles the selection state of a folder.
    /// </summary>
    /// <param name="folder">The folder to toggle.</param>
    private void ToggleSelection(OneDriveFolderNode folder)
    {
        ArgumentNullException.ThrowIfNull(folder);

        var newState = folder.SelectionState switch
        {
            SelectionState.Unchecked => true,
            SelectionState.Checked => false,
            SelectionState.Indeterminate => true,
            _ => true
        };

        _selectionService.SetSelection(folder, newState);
        _selectionService.UpdateParentStates(folder, [.. RootFolders]);

        // Save selections to database (fire and forget for UI responsiveness)
        if (!string.IsNullOrEmpty(SelectedAccountId))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _selectionService.SaveSelectionsToDatabaseAsync(SelectedAccountId, [.. RootFolders]);
                }
                catch
                {
                    // Silently ignore persistence errors to avoid disrupting UI
                }
            });
        }
    }

    /// <summary>
    /// Clears all folder selections.
    /// </summary>
    private void ClearSelections()
    {
        _selectionService.ClearAllSelections([.. RootFolders]);

        // Clear selections from database (fire and forget for UI responsiveness)
        if (!string.IsNullOrEmpty(SelectedAccountId))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _selectionService.SaveSelectionsToDatabaseAsync(SelectedAccountId, [.. RootFolders]);
                }
                catch
                {
                    // Silently ignore persistence errors to avoid disrupting UI
                }
            });
        }
    }

    /// <summary>
    /// Gets all selected folders.
    /// </summary>
    /// <returns>List of selected folder nodes.</returns>
    public List<OneDriveFolderNode> GetSelectedFolders()
    {
        return _selectionService.GetSelectedFolders([.. RootFolders]);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
