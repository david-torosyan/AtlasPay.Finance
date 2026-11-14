using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Accounts.Commands.UnfreezeAccount;

public sealed class UnfreezeAccountHandler(
    IAccountRepository accounts,
    IUnitOfWork unitOfWork)
{
    public async Task<bool> HandleAsync(
        UnfreezeAccountCommand command,
        CancellationToken ct = default)
    {
        var account = await accounts.GetByIdAsync(
            command.AccountId, ct);

        if (account is null)
            return false;

        account.Unfreeze();

        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}