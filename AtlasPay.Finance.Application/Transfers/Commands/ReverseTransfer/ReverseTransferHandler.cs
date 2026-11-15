
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Transfers.Exceptions;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Transfers.Entities;
using AtlasPay.Finance.Domain.Transfers.ValueObjects;

namespace AtlasPay.Finance.Application.Transfers.Commands.ReverseTransfer;

public sealed class ReverseTransferHandler(
    ITransferRepository transferRepository,
    ITransferReversalRepository reversalRepository,
    ILedgerTransactionRepository ledgerTransactionRepository,
    IAccountLockRepository accountLockRepository,
    ITransferLedgerRepository transferLedgerRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<ReverseTransferResult> HandleAsync(
        ReverseTransferCommand command,
        CancellationToken ct = default)
    {
        if (command.RequestId == Guid.Empty)
            throw new ArgumentException("RequestId is required.");

        if (command.TransferId == Guid.Empty)
            throw new ArgumentException("TransferId is required.");

        var reason = command.Reason?.Trim();

        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
            throw new ArgumentException("Invalid reversal reason.");

        var existing = await reversalRepository.GetByRequestIdAsync(
            command.RequestId, ct);

        if (existing is not null)
            return MatchExisting(existing, command);

        var reversal = TransferReversal.Create(
            command.RequestId,
            command.TransferId,
            reason);

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var transfer = await transferRepository.GetByIdAsync(
                        command.TransferId, token)
                        ?? throw new InvalidOperationException(
                            "Original transfer not found.");

                    if (transfer.Status != TransferStatus.Completed)
                        throw new InvalidOperationException(
                            "Only completed transfers can be reversed.");

                    // Lock accounts in a consistent order.
                    await accountLockRepository.LockAccountsAsync(
                        transfer.FromAccountId,
                        transfer.ToAccountId,
                        token);

                    var duplicate = await reversalRepository
                        .GetByRequestIdAsync(command.RequestId, token);

                    if (duplicate is not null)
                        throw new TransferReversalConflictException(
                            "Reversal request already exists.");

                    var previousReversal = await reversalRepository
                        .GetByOriginalTransferIdAsync(
                            transfer.Id, token);

                    if (previousReversal is not null)
                        throw new TransferReversalConflictException(
                            "Transfer has already been reversed.");

                    var originalLedger =
                        await ledgerTransactionRepository
                            .GetByReferenceIdAsync(transfer.Id, token)
                        ?? throw new InvalidOperationException(
                            "Original ledger transaction not found.");

                    if (!originalLedger.IsPosted ||
                        originalLedger.Type != LedgerTransactionType.Transfer)
                        throw new InvalidOperationException(
                            "Original ledger transaction is invalid.");

                    // Get customer ledger states.
                    var sender = await transferLedgerRepository
                        .GetByAccountIdAsync(
                            transfer.FromAccountId, token);

                    var recipient = await transferLedgerRepository
                        .GetByAccountIdAsync(
                            transfer.ToAccountId, token);

                    if (sender.LedgerAccount.Type != LedgerAccountType.Liability ||
                        recipient.LedgerAccount.Type != LedgerAccountType.Liability ||
                        sender.LedgerAccount.Currency != transfer.Amount.Currency ||
                        recipient.LedgerAccount.Currency != transfer.Amount.Currency ||
                        sender.Balance.Balance.Currency != transfer.Amount.Currency ||
                        recipient.Balance.Balance.Currency != transfer.Amount.Currency)
                    {
                        throw new InvalidOperationException(
                            "Transfer ledger configuration is invalid.");
                    }

                    // The original recipient must return the funds.
                    if (recipient.Balance.Balance.Amount
                        < transfer.Amount.Amount)
                    {
                        throw new InvalidOperationException(
                            "Recipient has insufficient funds for reversal.");
                    }

                    // Create opposite ledger postings.
                    var reversalLedger =
                        LedgerTransaction.CreateReversal(
                            reversal.Id,
                            originalLedger);

                    // Reverse customer balance movements.
                    sender.Balance.Credit(
                        transfer.Amount,
                        sender.LedgerAccount.Type);

                    recipient.Balance.Debit(
                        transfer.Amount,
                        recipient.LedgerAccount.Type);

                    await reversalRepository.AddAsync(
                        reversal, token);

                    await ledgerTransactionRepository.AddAsync(
                        reversalLedger, token);
                },
                ct);
        }
        catch (DuplicateTransferReversalRequestException)
        {
            var saved = await reversalRepository.GetByRequestIdAsync(
                command.RequestId, ct);

            if (saved is not null)
                return MatchExisting(saved, command);

            throw;
        }

        return new ReverseTransferResult(reversal.Id);
    }

    private static ReverseTransferResult MatchExisting(
        TransferReversal existing,
        ReverseTransferCommand command)
    {
        if (existing.OriginalTransferId != command.TransferId ||
            existing.Reason != command.Reason?.Trim())
        {
            throw new TransferReversalConflictException(
                "RequestId belongs to a different reversal.");
        }

        return new ReverseTransferResult(existing.Id);
    }
}
