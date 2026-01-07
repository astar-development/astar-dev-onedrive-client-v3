using AStarOneDriveClient.Repositories;
using AStarOneDriveClient.ViewModels;

namespace AStarOneDriveClient.Tests.Unit.ViewModels;

public class ViewSyncHistoryViewModelShould
{
    [Fact]
    public void ThrowArgumentNullExceptionWhenAccountRepositoryIsNull()
    {
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var exception = Should.Throw<ArgumentNullException>(() =>
            new ViewSyncHistoryViewModel(null!, mockFileOpLogRepo));

        exception.ParamName.ShouldBe("accountRepository");
    }

    [Fact]
    public void ThrowArgumentNullExceptionWhenFileOperationLogRepositoryIsNull()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();

        var exception = Should.Throw<ArgumentNullException>(() =>
            new ViewSyncHistoryViewModel(mockAccountRepo, null!));

        exception.ParamName.ShouldBe("fileOperationLogRepository");
    }

    [Fact]
    public void InitializeWithEmptyAccountsCollection()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.Accounts.ShouldBeEmpty();
    }

    [Fact]
    public void InitializeWithEmptySyncHistoryCollection()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.SyncHistory.ShouldBeEmpty();
    }

    [Fact]
    public void InitializeWithNullSelectedAccount()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.SelectedAccount.ShouldBeNull();
    }

    [Fact]
    public void InitializeWithCurrentPageOne()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.CurrentPage.ShouldBe(1);
    }

    [Fact]
    public void InitializeWithHasMoreRecordsTrue()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.HasMoreRecords.ShouldBeTrue();
    }

    [Fact]
    public void InitializeWithIsLoadingFalse()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public void InitializeLoadNextPageCommand()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.LoadNextPageCommand.ShouldNotBeNull();
    }

    [Fact]
    public void InitializeLoadPreviousPageCommand()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.LoadPreviousPageCommand.ShouldNotBeNull();
    }

    [Fact]
    public void ReturnCanGoToPreviousPageFalseWhenOnFirstPage()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.CanGoToPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public void ReturnCanGoToNextPageTrueWhenHasMoreRecords()
    {
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockFileOpLogRepo = Substitute.For<IFileOperationLogRepository>();

        var sut = new ViewSyncHistoryViewModel(mockAccountRepo, mockFileOpLogRepo);

        sut.CanGoToNextPage.ShouldBeTrue();
    }
}
