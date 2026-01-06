using AStarOneDriveClient.Models;
using AStarOneDriveClient.Models.Enums;
using AStarOneDriveClient.Repositories;
using AStarOneDriveClient.Services;
using NSubstitute;

namespace AStarOneDriveClient.Tests.Unit.Services;

public class SyncEngineShould
{
    [Fact]
    public async Task StartSyncAndReportProgress()
    {
        var (engine, mocks) = CreateTestEngine();
        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(new AccountInfo("acc1", "Test", @"C:\Sync", true, null, null));
        mocks.LocalScanner.ScanFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);
        mocks.FileMetadataRepo.GetByAccountIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns([]);

        var progressStates = new List<SyncState>();
        engine.Progress.Subscribe(progressStates.Add);

        await engine.StartSyncAsync("acc1");

        progressStates.Count.ShouldBeGreaterThan(0);
        progressStates.Last().Status.ShouldBe(SyncStatus.Completed);
    }

    [Fact]
    public async Task UploadNewLocalFiles()
    {
        var (engine, mocks) = CreateTestEngine();
        var localFile = new FileMetadata("", "acc1", "doc.txt", "/Documents/doc.txt", 100,
            DateTime.UtcNow, @"C:\Sync\Documents\doc.txt", null, null, "hash123",
            FileSyncStatus.PendingUpload, null);

        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(new AccountInfo("acc1", "Test", @"C:\Sync", true, null, null));
        mocks.LocalScanner.ScanFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([localFile]);
        mocks.FileMetadataRepo.GetByAccountIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns([]);

        await engine.StartSyncAsync("acc1");

        await mocks.FileMetadataRepo.Received(1).AddAsync(
            Arg.Is<FileMetadata>(f => f.Name == "doc.txt" && f.SyncStatus == FileSyncStatus.Synced),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateModifiedLocalFiles()
    {
        var (engine, mocks) = CreateTestEngine();
        var localFile = new FileMetadata("file1", "acc1", "doc.txt", "/Documents/doc.txt", 150,
            DateTime.UtcNow, @"C:\Sync\Documents\doc.txt", null, null, "newhash",
            FileSyncStatus.PendingUpload, null);
        var existingFile = new FileMetadata("file1", "acc1", "doc.txt", "/Documents/doc.txt", 100,
            DateTime.UtcNow.AddDays(-1), @"C:\Sync\Documents\doc.txt", null, null, "oldhash",
            FileSyncStatus.Synced, SyncDirection.Upload);

        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(new AccountInfo("acc1", "Test", @"C:\Sync", true, null, null));
        mocks.LocalScanner.ScanFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([localFile]);
        mocks.FileMetadataRepo.GetByAccountIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns([existingFile]);

        await engine.StartSyncAsync("acc1");

        await mocks.FileMetadataRepo.Received(1).UpdateAsync(
            Arg.Is<FileMetadata>(f => f.LocalHash == "newhash" && f.SyncStatus == FileSyncStatus.Synced),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SkipUnchangedFiles()
    {
        var (engine, mocks) = CreateTestEngine();
        var localFile = new FileMetadata("file1", "acc1", "doc.txt", "/Documents/doc.txt", 100,
            DateTime.UtcNow, @"C:\Sync\Documents\doc.txt", null, null, "hash123",
            FileSyncStatus.Synced, null);

        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(new AccountInfo("acc1", "Test", @"C:\Sync", true, null, null));
        mocks.LocalScanner.ScanFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([localFile]);
        mocks.FileMetadataRepo.GetByAccountIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns([localFile]);

        await engine.StartSyncAsync("acc1");

        await mocks.FileMetadataRepo.DidNotReceive().AddAsync(Arg.Any<FileMetadata>(), Arg.Any<CancellationToken>());
        await mocks.FileMetadataRepo.DidNotReceive().UpdateAsync(Arg.Any<FileMetadata>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleNoSelectedFolders()
    {
        var (engine, mocks) = CreateTestEngine();
        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns([]);

        var progressStates = new List<SyncState>();
        engine.Progress.Subscribe(progressStates.Add);

        await engine.StartSyncAsync("acc1");

        progressStates.Last().Status.ShouldBe(SyncStatus.Idle);
    }

    [Fact]
    public async Task HandleAccountNotFound()
    {
        var (engine, mocks) = CreateTestEngine();
        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns((AccountInfo?)null);

        var progressStates = new List<SyncState>();
        engine.Progress.Subscribe(progressStates.Add);

        await engine.StartSyncAsync("acc1");

        progressStates.Last().Status.ShouldBe(SyncStatus.Failed);
    }

    [Fact]
    public async Task HandleCancellation()
    {
        var (engine, mocks) = CreateTestEngine();
        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(new AccountInfo("acc1", "Test", @"C:\Sync", true, null, null));

        var cts = new CancellationTokenSource();
        mocks.LocalScanner.ScanFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<FileMetadata>>(new OperationCanceledException(cts.Token)));
        cts.Cancel();

        var progressStates = new List<SyncState>();
        engine.Progress.Subscribe(progressStates.Add);

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await engine.StartSyncAsync("acc1", cts.Token));

        progressStates.Last().Status.ShouldBe(SyncStatus.Paused);
    }

    [Fact]
    public async Task ReportProgressWithFileCountsAndBytes()
    {
        var (engine, mocks) = CreateTestEngine();
        var files = new[]
        {
            new FileMetadata("", "acc1", "file1.txt", "/Documents/file1.txt", 1000,
                DateTime.UtcNow, @"C:\Sync\Documents\file1.txt", null, null, "hash1",
                FileSyncStatus.PendingUpload, null),
            new FileMetadata("", "acc1", "file2.txt", "/Documents/file2.txt", 2000,
                DateTime.UtcNow, @"C:\Sync\Documents\file2.txt", null, null, "hash2",
                FileSyncStatus.PendingUpload, null)
        };

        mocks.SyncConfigRepo.GetSelectedFoldersAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(["/Documents"]);
        mocks.AccountRepo.GetByIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns(new AccountInfo("acc1", "Test", @"C:\Sync", true, null, null));
        mocks.LocalScanner.ScanFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(files);
        mocks.FileMetadataRepo.GetByAccountIdAsync("acc1", Arg.Any<CancellationToken>())
            .Returns([]);

        var progressStates = new List<SyncState>();
        engine.Progress.Subscribe(progressStates.Add);

        await engine.StartSyncAsync("acc1");

        var finalState = progressStates.Last();
        finalState.TotalFiles.ShouldBe(2);
        finalState.CompletedFiles.ShouldBe(2);
        finalState.TotalBytes.ShouldBe(3000);
        finalState.CompletedBytes.ShouldBe(3000);
    }

    private static (SyncEngine Engine, TestMocks Mocks) CreateTestEngine()
    {
        ILocalFileScanner localScanner = Substitute.For<ILocalFileScanner>();
        IRemoteChangeDetector remoteDetector = Substitute.For<IRemoteChangeDetector>();
        IFileMetadataRepository fileMetadataRepo = Substitute.For<IFileMetadataRepository>();
        ISyncConfigurationRepository syncConfigRepo = Substitute.For<ISyncConfigurationRepository>();
        IAccountRepository accountRepo = Substitute.For<IAccountRepository>();

        var engine = new SyncEngine(localScanner, remoteDetector, fileMetadataRepo, syncConfigRepo, accountRepo);
        var mocks = new TestMocks(localScanner, remoteDetector, fileMetadataRepo, syncConfigRepo, accountRepo);

        return (engine, mocks);
    }

    private sealed record TestMocks(
        ILocalFileScanner LocalScanner,
        IRemoteChangeDetector RemoteDetector,
        IFileMetadataRepository FileMetadataRepo,
        ISyncConfigurationRepository SyncConfigRepo,
        IAccountRepository AccountRepo);
}
