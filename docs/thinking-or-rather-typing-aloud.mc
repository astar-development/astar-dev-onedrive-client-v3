# Thinking / Typing "Aloud"

FileOperation                                                                                       Output/Input type
User selects folder(s) to sync                                                                      IList<string>
OneDrive is checked for selected folder content - details are added to the database                 IList<OneDriveItem>
Local sync folder is checked for all existing content and database is updated                       IList<LocalFileItem>
Database is checked for deletions - either local or OneDrive                                        IList<Deletions>
Deletions are applied to upload lists                                                               IList<FileUpload?>
Deletions are applied to download lists                                                             IList<FileDownload?>
Conflicts are detected and flagged                                                                  db update
OneDrive content that does not exist locally is downloaded and database updated                     IList<FileUpload?>
Local folder(s) with content that does not exist on OneDrive is uploaded and database updated       IList<FileDownload?>
Completion is reported

Throughout the process, the UI MUST get regular updates

Current Enums:

FileSyncStatus
    Synced = 0,
    PendingUpload = 1,
    PendingDownload = 2,
    Uploading = 3,
    Downloading = 4,
    Conflict = 5,
    Failed = 6,
    Deleted = 7

Sync Status/
    Idle = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Queued = 5

SyncDirection
    Upload = 0,
    Download = 1

FileOperation
    Upload = 0,
    Download = 1,
    Delete = 2,
    ConflictDetected = 3

FileChangeType
    Created,
    Modified,
    Deleted,
    Renamed

ConflictResolutionStrategy
    None = 0,
    KeepLocal = 1,
    KeepRemote = 2,
    KeepBoth = 3,
    KeepNewer = 4
