
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Deposits.Exceptions;
using AtlasPay.Finance.Application.Deposits.IntegrationEvents;
using AtlasPay.Finance.Application.IntegrationEvents;
using AtlasPay.Finance.Application.Transfers.IntegrationEvents;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Deposits.Entities;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Outbox.Entities;
using System.Text.Json;

namespace AtlasPay.Finance.Application.Deposits.Commands.DepositMoney;

public sealed class DepositMoneyHandler(
    IAccountLockRepository accountLockRepository,
    ITransferLedgerRepository transferLedgerRepository,
    IInternalLedgerRepository internalLedgerRepository,
    IAtomicBalanceRepository atomicBalanceRepository,
    IDepositRepository depositRepository,
    ILedgerTransactionRepository ledgerTransactionRepository,
    IUnitOfWork unitOfWork,
    IOutboxRepository outboxRepository)
{
    public async Task<DepositMoneyResult> HandleAsync(
        DepositMoneyCommand command,
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
                "Deposit amount must be positive.");

        var existing = await depositRepository.GetByRequestIdAsync(
            command.RequestId,
            ct);

        if (existing is not null)
            return MatchExisting(existing, command);

        var deposit = Deposit.Create(
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

                    var duplicate = await depositRepository
                        .GetByRequestIdAsync(
                            command.RequestId,
                            token);

                    if (duplicate is not null)
                        throw new DuplicateDepositRequestException();

                    if (account.Status != AccountStatus.Active)
                        throw new InvalidOperationException(
                            "Customer account must be active.");

                    if (account.Currency != amount.Currency)
                        throw new InvalidOperationException(
                            "Deposit currency does not match account currency.");

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

                    var ledgerTransaction = LedgerTransaction.Create(
                            deposit.Id,
                            LedgerTransactionType.Deposit);

                    ledgerTransaction.AddPosting(
                        cashLedger.LedgerAccount.Id,
                        PostingDirection.Debit,
                        amount);

                    ledgerTransaction.AddPosting(
                        customerLedger.LedgerAccount.Id,
                        PostingDirection.Credit,
                        amount);

                    ledgerTransaction.Post();

                    customerLedger.Balance.Credit(
                        amount,
                        customerLedger.LedgerAccount.Type);

                    deposit.Complete();

                    await depositRepository.AddAsync(
                        deposit,
                        token);

                    await ledgerTransactionRepository.AddAsync(
                        ledgerTransaction,
                        token);

                    await atomicBalanceRepository.IncreaseAsync(
                        cashLedger.LedgerAccount.Id,
                        amount,
                        token);

                    var eventId = Guid.NewGuid();
                    var integrationEvent = new DepositCompletedV1(
                        EventId: eventId,
                        DepositId: deposit.Id,
                        AccountId: deposit.AccountId,
                        Amount: deposit.Amount.Amount,
                        Currency: deposit.Amount.Currency.ToString());

                    var payload = JsonSerializer.Serialize(
                        integrationEvent,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    var outboxMessage = OutboxMessage.Create(
                        eventId,
                        FinanceEventTypes.DepositCompleted,
                        payload);

                    await outboxRepository.AddAsync(outboxMessage, token);
                },
                ct);
        }
        catch (DuplicateDepositRequestException)
        {
            return await ResolveDuplicateAsync(command, ct);
        }

        return new DepositMoneyResult(deposit.Id);
    }

    private async Task<DepositMoneyResult> ResolveDuplicateAsync(
        DepositMoneyCommand command,
        CancellationToken ct)
    {
        var existing = await depositRepository.GetByRequestIdAsync(
            command.RequestId,
            ct)
            ?? throw new InvalidOperationException(
                "Deposit idempotency result could not be resolved.");

        return MatchExisting(existing, command);
    }

    private static DepositMoneyResult MatchExisting(
        Deposit existing,
        DepositMoneyCommand command)
    {
        if (existing.AccountId != command.AccountId ||
            existing.Amount.Amount != command.Amount ||
            existing.Amount.Currency != command.Currency)
        {
            throw new DepositRequestConflictException();
        }

        return new DepositMoneyResult(existing.Id);
    }
}
