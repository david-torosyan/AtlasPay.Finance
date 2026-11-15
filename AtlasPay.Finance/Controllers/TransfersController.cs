using AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;
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
}
