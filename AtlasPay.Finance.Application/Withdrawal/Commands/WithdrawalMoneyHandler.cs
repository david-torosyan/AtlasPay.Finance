using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Withdrawals.Exceptions;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Outbox.Entities;
using AtlasPay.Finance.Domain.Withdrawals.Entities;
using System.Text.Json;

namespace AtlasPay.Finance.Application.Withdrawals.Commands;

public sealed class WithdrawalMoneyHandler(
    IAccountLockRepository accountLockRepository,
    ITransferLedgerRepository transferLedgerRepository,
    IInternalLedgerRepository internalLedgerRepository,
    IAtomicBalanceRepository atomicBalanceRepository,
    IWithdrawalRepository withdrawalRepository,
    ILedgerTransactionRepository ledgerTransactionRepository,
    IUnitOfWork unitOfWork,
    IOutboxRepository outboxRepository)
{
    public async Task<WithdrawalMoneyResult> HandleAsync(
        WithdrawalMoneyCommand command,
        CancellationToken ct = default)
    {
        if (command.RequestId == Guid.Empty)
            throw new ArgumentException(
                "RequestId is required.");

        if (command.AccountId == Guid.Empty)
            throw new ArgumentException(
                "AccountId is required.");

        var amount = new Money(
            command.Amount,
            command.Currency);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Withdrawal amount must be positive.");

        var existing = await withdrawalRepository
                .GetByRequestIdAsync(command.RequestId, ct);

        if (existing is not null)
            return MatchExisting(existing, command);

        var withdrawal = Withdrawal.Create(
            command.RequestId,
            command.AccountId,
            amount);

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var account = await accountLockRepository
                        .LockAccountAsync(
                            command.AccountId,
                            token);

                    var duplicate = await withdrawalRepository
                       .GetByRequestIdAsync(
                           command.RequestId,
                           token);

                    if (duplicate is not null)
                        throw new DuplicateWithdrawalRequestException();

                    if (account.Status != AccountStatus.Active)
                        throw new InvalidOperationException(
                            "Customer account must be active.");

                    if (account.Currency != amount.Currency)
                        throw new InvalidOperationException(
                            "Withdrawal currency does not match account currency.");

                    var customerLedger = await transferLedgerRepository
                        .GetByAccountIdAsync(
                            account.Id,
                            token);

                    if (customerLedger.LedgerAccount.Type
                        != LedgerAccountType.Liability)
                        throw new InvalidOperationException(
                            "Customer ledger must be a liability.");

                    if (customerLedger.LedgerAccount.Currency
                            != amount.Currency ||
                        customerLedger.Balance.Balance.Currency
                            != amount.Currency)
                        throw new InvalidOperationException(
                            "Customer ledger currency mismatch.");

                    var cashCode = amount.Currency switch
                    {
                        Currency.AMD => LedgerAccountCode.CashAmd,
                        Currency.USD => LedgerAccountCode.CashUsd,
                        Currency.EUR => LedgerAccountCode.CashEur,
                        _ => throw new ArgumentOutOfRangeException(
                            nameof(command.Currency))
                    };

                    var cashLedger = await internalLedgerRepository
                        .GetByCodeAsync(
                            cashCode,
                            token);

                    if (cashLedger.LedgerAccount.Type
                            != LedgerAccountType.Asset ||
                        cashLedger.LedgerAccount.AccountId is not null ||
                        cashLedger.LedgerAccount.Currency
                            != amount.Currency ||
                        cashLedger.Balance.Balance.Currency
                            != amount.Currency)
                        throw new InvalidOperationException(
                            "Bank cash ledger is misconfigured.");

                    var ledgerTransaction =
                        LedgerTransaction.Create(
                            withdrawal.Id,
                            LedgerTransactionType.Withdrawal);

                    ledgerTransaction.AddPosting(
                        cashLedger.LedgerAccount.Id,
                        PostingDirection.Credit,
                        amount);

                    ledgerTransaction.AddPosting(
                        customerLedger.LedgerAccount.Id,
                        PostingDirection.Debit,
                        amount);

                    if (customerLedger.Balance.Balance.Amount < amount.Amount)
                    {
                        throw new InvalidOperationException(
                            "Insufficient customer funds.");
                    }

                    ledgerTransaction.Post();

                    customerLedger.Balance.Debit(
                        amount,
                        customerLedger.LedgerAccount.Type);

                    withdrawal.Complete();

                    await withdrawalRepository.AddAsync(
                        withdrawal,
                        token);

                    await ledgerTransactionRepository.AddAsync(
                        ledgerTransaction,
                        token);

                    var cashDecreased = await atomicBalanceRepository.TryDecreaseAsync(
                                            cashLedger.LedgerAccount.Id,
                                            amount,
                                            token);
                    if (!cashDecreased)
                    {
                        throw new InvalidOperationException(
                            "Insufficient bank cash.");
                    }


                    var eventId = Guid.NewGuid();

                    var integrationEvent = new WithdrawalCompletedV2(
                        EventId: eventId,
                        WithdrawalId: withdrawal.Id,
                        AccountId: withdrawal.AccountId,
                        CustomerId: account.CustomerId,
                        Amount: withdrawal.Amount.Amount,
                        Currency: withdrawal.Amount.Currency.ToString(),
                        OccurredAt: DateTime.UtcNow);

                    var payload = JsonSerializer.Serialize(
                        integrationEvent,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    var outboxMessage = OutboxMessage.Create(
                        eventId,
                        FinanceEventTypes.WithdrawalCompletedV2,
                        payload);

                    await outboxRepository.AddAsync(
                        outboxMessage,
                        token);
                },
                ct);
        }
        catch (DuplicateWithdrawalRequestException)
        {
            return await ResolveDuplicateAsync(command, ct);
        }

        return new WithdrawalMoneyResult(withdrawal.Id);
    }

    private async Task<WithdrawalMoneyResult> ResolveDuplicateAsync(
        WithdrawalMoneyCommand command,
        CancellationToken ct)
    {
        var existing = await withdrawalRepository.GetByRequestIdAsync(
            command.RequestId,
            ct)
            ?? throw new InvalidOperationException(
                "Withdrawal idempotency result could not be resolved.");

        return MatchExisting(existing, command);
    }

    private static WithdrawalMoneyResult MatchExisting(
        Withdrawal existing,
        WithdrawalMoneyCommand command)
    {
        if (existing.AccountId != command.AccountId ||
            existing.Amount.Amount != command.Amount ||
            existing.Amount.Currency != command.Currency)
        {
            throw new WithdrawalRequestConflictException();
        }

        return new WithdrawalMoneyResult(existing.Id);
    }
}
