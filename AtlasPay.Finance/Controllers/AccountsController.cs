using AtlasPay.Finance.Application.Accounts.Commands.ActivateAccount;
using AtlasPay.Finance.Application.Accounts.Commands.CreateAccount;
using AtlasPay.Finance.Application.Accounts.Commands.FreezeAccount;
using AtlasPay.Finance.Application.Accounts.Commands.UnfreezeAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;
using AtlasPay.Finance.Contracts.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Finance.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : Controller
{
    private readonly CreateAccountHandler _createHandler;
    private readonly GetAccountHandler _getHandler;

    public AccountsController(
       CreateAccountHandler createHandler,
       GetAccountHandler getHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
    }


    [HttpPost]
    public async Task<ActionResult<CreateAccountResponse>> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(
            request.CustomerId,
            request.Currency);

        var accountId = await _createHandler.HandleAsync(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateAccountResponse(accountId));
    }

    [HttpGet("{accountId:guid}")]
    public async Task<ActionResult<GetAccountResponse>> GetById(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountQuery(accountId);

        var result = await _getHandler.HandleAsync(
            query,
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(new GetAccountResponse(
            result.AccountId,
            result.CustomerId,
            result.Currency.ToString(),
            result.Status.ToString(),
            result.Balance));
    }

    [HttpPost("{accountId:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid accountId,
        [FromServices] ActivateAccountHandler handler,
        CancellationToken ct)
    {
        try
        {
            var found = await handler.HandleAsync(
                new ActivateAccountCommand(accountId), ct);

            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Invalid account transition",
                Detail = ex.Message
            });
        }
    }

    [HttpPost("{accountId:guid}/freeze")]
    public async Task<IActionResult> Freeze(
        Guid accountId,
        [FromServices] FreezeAccountHandler handler,
        CancellationToken ct)
    {
        try
        {
            var found = await handler.HandleAsync(
                new FreezeAccountCommand(accountId), ct);

            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Invalid account transition",
                Detail = ex.Message
            });
        }
    }

    [HttpPost("{accountId:guid}/unfreeze")]
    public async Task<IActionResult> Unfreeze(
        Guid accountId,
        [FromServices] UnfreezeAccountHandler handler,
        CancellationToken ct)
    {
        try
        {
            var found = await handler.HandleAsync(
                new UnfreezeAccountCommand(accountId), ct);

            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Invalid account transition",
                Detail = ex.Message
            });
        }
    }

    [HttpGet("{accountId:guid}/balance")]
    public async Task<IActionResult> GetBalance(
        Guid accountId,
        [FromServices] GetAccountBalanceHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new GetAccountBalanceQuery(accountId),
            ct);

        if (result is null)
            return NotFound();

        return Ok(new
        {
            result.AccountId,
            Currency = result.Currency.ToString(),
            result.LedgerBalance
        });
    }
}
