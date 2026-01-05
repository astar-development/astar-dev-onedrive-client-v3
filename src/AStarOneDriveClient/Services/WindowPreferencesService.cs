using AStarOneDriveClient.Data;
using AStarOneDriveClient.Data.Entities;
using AStarOneDriveClient.Models;
using Microsoft.EntityFrameworkCore;

namespace AStarOneDriveClient.Services;

/// <summary>
/// Service for managing window position and size preferences using database storage.
/// </summary>
public sealed class WindowPreferencesService : IWindowPreferencesService
{
    private readonly SyncDbContext _context;

    public WindowPreferencesService(SyncDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<WindowPreferences?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _context.WindowPreferences
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : MapToModel(entity);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(WindowPreferences preferences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var entity = await _context.WindowPreferences.FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            entity = new WindowPreferencesEntity
            {
                X = preferences.X,
                Y = preferences.Y,
                Width = preferences.Width,
                Height = preferences.Height,
                IsMaximized = preferences.IsMaximized
            };
            _context.WindowPreferences.Add(entity);
        }
        else
        {
            entity.X = preferences.X;
            entity.Y = preferences.Y;
            entity.Width = preferences.Width;
            entity.Height = preferences.Height;
            entity.IsMaximized = preferences.IsMaximized;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static WindowPreferences MapToModel(WindowPreferencesEntity entity) =>
        new(
            entity.Id,
            entity.X,
            entity.Y,
            entity.Width,
            entity.Height,
            entity.IsMaximized
        );
}
