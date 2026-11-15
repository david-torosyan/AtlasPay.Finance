using AtlasPay.Finance.Api.Contracts.Transfers;
using AtlasPay.Finance.Application.Transfers.Commands.ReverseTransfer;
using AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;
using AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;
using AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;
using AtlasPay.Finance.Contracts.Transfers;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Finance.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public sealed class TransfersController : Controller
{
    [HttpPost]
    public async Task<ActionResult<TransferMoneyResponse>> Transfer(
        [FromBody] TransferMoneyRequest request,
        [FromServices] TransferMoneyHandler handler,
        CancellationToken ct)
    {
        var command = new TransferMoneyCommand(
            request.RequestId,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Currency);

        var transferId = await handler.HandleAsync(command, ct);

        return Ok(new TransferMoneyResponse(transferId));
    }

    [HttpGet("{transferId:guid}")]
    public async Task<IActionResult> GetById(
    Guid transferId,
    [FromServices] GetTransferHandler handler,
    CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new GetTransferQuery(transferId),
            ct);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpGet("{accountId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid accountId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] GetAccountTransactionsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new GetAccountTransactionsQuery(
                accountId,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize),
            ct);

        return Ok(result);
    }

    [HttpPost("{transferId:guid}/reverse")]
    public async Task<ActionResult<ReverseTransferResult>> Reverse(
        [FromRoute] Guid transferId,
        [FromBody] ReverseTransferRequest request,
        [FromServices] ReverseTransferHandler handler,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        var command = new ReverseTransferCommand(
            request.RequestId,
            transferId,
            request.Reason);

        var result = await handler.HandleAsync(command, ct);

        return Ok(result);
    }
}