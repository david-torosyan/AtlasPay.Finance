using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Accounts.Commands.ActivateAccount;


public sealed class ActivateAccountHandler(
    IAccountRepository accounts,
    IUnitOfWork unitOfWork)
{
    public async Task<bool> HandleAsync(
        ActivateAccountCommand command,
        CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync
                (command.AccountId, ct);

        if (account == null)
            return false;

        account.Activate();

        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}