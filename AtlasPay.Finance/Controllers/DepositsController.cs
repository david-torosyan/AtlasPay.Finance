using AtlasPay.Finance.Api.Contracts.Deposits;
using AtlasPay.Finance.Application.Deposits.Commands.DepositMoney;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Finance.Api.Controllers;

[ApiController]
[Route("api/deposits")]
public class DepositsController(
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DepositMoneyResult>> Deposit(
        [FromBody] DepositMoneyRequest request,
        [FromServices] DepositMoneyHandler handler,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        var command = new DepositMoneyCommand(
            request.RequestId,
            request.AccountId,
            request.Amount,
            request.Currency);

        var result = await handler.HandleAsync(command, ct);

        return Ok(result);
    }
}
