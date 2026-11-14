using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Accounts.Commands.FreezeAccount;

public sealed class FreezeAccountHandler(
    IAccountRepository accounts,
    IUnitOfWork unitOfWork)
{
    public async Task<bool> HandleAsync(
        FreezeAccountCommand command,
        CancellationToken ct = default)
    {
        var account = await accounts.GetByIdAsync(
            command.AccountId, ct);

        if (account is null)
            return false;

        account.Freeze();

        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}