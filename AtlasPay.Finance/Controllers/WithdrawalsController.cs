
using AtlasPay.Finance.Api.Contracts.Withdrawals;
using AtlasPay.Finance.Application.Withdrawals.Commands;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Finance.Api.Controllers;

[ApiController]
[Route("api/withdrawals")]
public sealed class WithdrawalsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WithdrawalMoneyResult>> Withdraw(
        [FromBody] WithdrawalMoneyRequest request,
        [FromServices] WithdrawalMoneyHandler handler,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        var command = new WithdrawalMoneyCommand(
            request.RequestId,
            request.AccountId,
            request.Amount,
            request.Currency);

        var result = await handler.HandleAsync(command, ct);

        return Ok(result);
    }
}
