using System.Reactive.Linq;
using AStarOneDriveClient.Models;
using AStarOneDriveClient.ViewModels;

namespace AStarOneDriveClient.Tests.Unit.ViewModels;

public class AccountManagementViewModelShould
{
    [Fact]
    public void InitializeWithEmptyAccountCollection()
    {
        using var viewModel = new AccountManagementViewModel();

        viewModel.Accounts.ShouldBeEmpty();
    }

    [Fact]
    public void InitializeWithNullSelectedAccount()
    {
        using var viewModel = new AccountManagementViewModel();

        viewModel.SelectedAccount.ShouldBeNull();
    }

    [Fact]
    public void InitializeWithIsLoadingFalse()
    {
        using var viewModel = new AccountManagementViewModel();

        viewModel.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public void RaisePropertyChangedWhenSelectedAccountChanges()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1");
        var propertyChanged = false;

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AccountManagementViewModel.SelectedAccount))
            {
                propertyChanged = true;
            }
        };

        viewModel.SelectedAccount = account;

        propertyChanged.ShouldBeTrue();
        viewModel.SelectedAccount.ShouldBe(account);
    }

    [Fact]
    public void NotRaisePropertyChangedWhenSettingSameSelectedAccount()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1");
        viewModel.SelectedAccount = account;

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (_, _) => propertyChangedCount++;

        viewModel.SelectedAccount = account;

        propertyChangedCount.ShouldBe(0);
    }

    [Fact]
    public void RaisePropertyChangedWhenIsLoadingChanges()
    {
        using var viewModel = new AccountManagementViewModel();
        var propertyChanged = false;

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AccountManagementViewModel.IsLoading))
            {
                propertyChanged = true;
            }
        };

        viewModel.IsLoading = true;

        propertyChanged.ShouldBeTrue();
        viewModel.IsLoading.ShouldBeTrue();
    }

    [Fact]
    public void NotRaisePropertyChangedWhenSettingSameIsLoadingValue()
    {
        using var viewModel = new AccountManagementViewModel();
        viewModel.IsLoading = true;

        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (_, _) => propertyChangedCount++;

        viewModel.IsLoading = true;

        propertyChangedCount.ShouldBe(0);
    }

    [Fact]
    public void HaveAddAccountCommandAlwaysEnabled()
    {
        using var viewModel = new AccountManagementViewModel();

        var canExecute = viewModel.AddAccountCommand.CanExecute.FirstAsync().Wait();

        canExecute.ShouldBeTrue();
    }

    [Fact]
    public void ExecuteAddAccountCommandSuccessfully()
    {
        using var viewModel = new AccountManagementViewModel();

        Should.NotThrow(() => viewModel.AddAccountCommand.Execute().Subscribe());
    }

    [Fact]
    public void HaveRemoveAccountCommandDisabledWhenNoAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();

        var canExecute = viewModel.RemoveAccountCommand.CanExecute.FirstAsync().Wait();

        canExecute.ShouldBeFalse();
    }

    [Fact]
    public void EnableRemoveAccountCommandWhenAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1");

        viewModel.SelectedAccount = account;

        var canExecute = viewModel.RemoveAccountCommand.CanExecute.FirstAsync().Wait();
        canExecute.ShouldBeTrue();
    }

    [Fact]
    public void DisableRemoveAccountCommandWhenAccountDeselected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1");
        viewModel.SelectedAccount = account;

        viewModel.SelectedAccount = null;

        var canExecute = viewModel.RemoveAccountCommand.CanExecute.FirstAsync().Wait();
        canExecute.ShouldBeFalse();
    }

    [Fact]
    public void ExecuteRemoveAccountCommandWhenAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1");
        viewModel.SelectedAccount = account;

        Should.NotThrow(() => viewModel.RemoveAccountCommand.Execute().Subscribe());
    }

    [Fact]
    public void HaveLoginCommandDisabledWhenNoAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();

        var canExecute = viewModel.LoginCommand.CanExecute.FirstAsync().Wait();

        canExecute.ShouldBeFalse();
    }

    [Fact]
    public void EnableLoginCommandWhenUnauthenticatedAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1", isAuthenticated: false);

        viewModel.SelectedAccount = account;

        var canExecute = viewModel.LoginCommand.CanExecute.FirstAsync().Wait();
        canExecute.ShouldBeTrue();
    }

    [Fact]
    public void DisableLoginCommandWhenAuthenticatedAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1", isAuthenticated: true);

        viewModel.SelectedAccount = account;

        var canExecute = viewModel.LoginCommand.CanExecute.FirstAsync().Wait();
        canExecute.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteLoginCommandSuccessfully()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1", isAuthenticated: false);
        viewModel.SelectedAccount = account;

        await Should.NotThrowAsync(async () => await viewModel.LoginCommand.Execute());
    }

    [Fact]
    public void HaveLogoutCommandDisabledWhenNoAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();

        var canExecute = viewModel.LogoutCommand.CanExecute.FirstAsync().Wait();

        canExecute.ShouldBeFalse();
    }

    [Fact]
    public void EnableLogoutCommandWhenAuthenticatedAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1", isAuthenticated: true);

        viewModel.SelectedAccount = account;

        var canExecute = viewModel.LogoutCommand.CanExecute.FirstAsync().Wait();
        canExecute.ShouldBeTrue();
    }

    [Fact]
    public void DisableLogoutCommandWhenUnauthenticatedAccountSelected()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1", isAuthenticated: false);

        viewModel.SelectedAccount = account;

        var canExecute = viewModel.LogoutCommand.CanExecute.FirstAsync().Wait();
        canExecute.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteLogoutCommandSuccessfully()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1", isAuthenticated: true);
        viewModel.SelectedAccount = account;

        await Should.NotThrowAsync(async () => await viewModel.LogoutCommand.Execute());
    }

    [Fact]
    public void AllowAddingAccountsToCollection()
    {
        using var viewModel = new AccountManagementViewModel();
        var account1 = CreateAccount("acc1", "User 1");
        var account2 = CreateAccount("acc2", "User 2");

        viewModel.Accounts.Add(account1);
        viewModel.Accounts.Add(account2);

        viewModel.Accounts.Count.ShouldBe(2);
        viewModel.Accounts.ShouldContain(account1);
        viewModel.Accounts.ShouldContain(account2);
    }

    [Fact]
    public void AllowRemovingAccountsFromCollection()
    {
        using var viewModel = new AccountManagementViewModel();
        var account = CreateAccount("acc1", "User 1");
        viewModel.Accounts.Add(account);

        viewModel.Accounts.Remove(account);

        viewModel.Accounts.ShouldBeEmpty();
    }

    [Fact]
    public void DisposeSuccessfullyWithoutErrors()
    {
        var viewModel = new AccountManagementViewModel();

        Should.NotThrow(() => viewModel.Dispose());
    }

    private static AccountInfo CreateAccount(string id, string displayName, bool isAuthenticated = false) =>
        new(id, displayName, $@"C:\Sync\{id}", isAuthenticated, null, null);
}
