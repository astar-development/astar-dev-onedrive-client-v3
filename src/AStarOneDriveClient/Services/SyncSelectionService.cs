using AStarOneDriveClient.Models;

namespace AStarOneDriveClient.Services;

/// <summary>
/// Service for managing folder selection state in the sync tree.
/// </summary>
public sealed class SyncSelectionService : ISyncSelectionService
{
    /// <inheritdoc/>
    public void SetSelection(OneDriveFolderNode folder, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(folder);

        var selectionState = isSelected ? SelectionState.Checked : SelectionState.Unchecked;
        folder.SelectionState = selectionState;
        folder.IsSelected = isSelected;

        // Cascade to all children
        CascadeSelectionToChildren(folder, selectionState);
    }

    /// <inheritdoc/>
    public void UpdateParentStates(OneDriveFolderNode folder, List<OneDriveFolderNode> rootFolders)
    {
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(rootFolders);

        if (folder.ParentId is null)
            return;

        var parent = FindNodeById(rootFolders, folder.ParentId);
        if (parent is null)
            return;

        var calculatedState = CalculateStateFromChildren(parent);
        parent.SelectionState = calculatedState;
        parent.IsSelected = calculatedState switch
        {
            SelectionState.Checked => true,
            SelectionState.Unchecked => false,
            SelectionState.Indeterminate => null,
            _ => null
        };

        // Continue propagating upward
        UpdateParentStates(parent, rootFolders);
    }

    /// <inheritdoc/>
    public List<OneDriveFolderNode> GetSelectedFolders(List<OneDriveFolderNode> rootFolders)
    {
        ArgumentNullException.ThrowIfNull(rootFolders);

        var selectedFolders = new List<OneDriveFolderNode>();
        CollectSelectedFolders(rootFolders, selectedFolders);
        return selectedFolders;
    }

    /// <inheritdoc/>
    public void ClearAllSelections(List<OneDriveFolderNode> rootFolders)
    {
        ArgumentNullException.ThrowIfNull(rootFolders);

        foreach (var folder in rootFolders)
        {
            SetSelection(folder, false);
        }
    }

    /// <inheritdoc/>
    public SelectionState CalculateStateFromChildren(OneDriveFolderNode folder)
    {
        ArgumentNullException.ThrowIfNull(folder);

        if (folder.Children.Count == 0)
            return folder.SelectionState;

        var checkedCount = 0;
        var uncheckedCount = 0;
        var indeterminateCount = 0;

        foreach (var child in folder.Children)
        {
            switch (child.SelectionState)
            {
                case SelectionState.Checked:
                    checkedCount++;
                    break;
                case SelectionState.Unchecked:
                    uncheckedCount++;
                    break;
                case SelectionState.Indeterminate:
                    indeterminateCount++;
                    break;
            }
        }

        // If any child is indeterminate, parent is indeterminate
        if (indeterminateCount > 0)
            return SelectionState.Indeterminate;

        // If all children are checked, parent is checked
        if (checkedCount == folder.Children.Count)
            return SelectionState.Checked;

        // If all children are unchecked, parent is unchecked
        if (uncheckedCount == folder.Children.Count)
            return SelectionState.Unchecked;

        // Mixed state = indeterminate
        return SelectionState.Indeterminate;
    }

    private static void CascadeSelectionToChildren(OneDriveFolderNode folder, SelectionState state)
    {
        foreach (var child in folder.Children)
        {
            child.SelectionState = state;
            child.IsSelected = state switch
            {
                SelectionState.Checked => true,
                SelectionState.Unchecked => false,
                _ => null
            };

            CascadeSelectionToChildren(child, state);
        }
    }

    private static void CollectSelectedFolders(List<OneDriveFolderNode> folders, List<OneDriveFolderNode> result)
    {
        foreach (var folder in folders)
        {
            if (folder.SelectionState == SelectionState.Checked)
            {
                result.Add(folder);
            }

            CollectSelectedFolders(folder.Children.ToList(), result);
        }
    }

    private static OneDriveFolderNode? FindNodeById(List<OneDriveFolderNode> folders, string nodeId)
    {
        foreach (var folder in folders)
        {
            if (folder.Id == nodeId)
                return folder;

            var foundInChildren = FindNodeById(folder.Children.ToList(), nodeId);
            if (foundInChildren is not null)
                return foundInChildren;
        }

        return null;
    }
}
