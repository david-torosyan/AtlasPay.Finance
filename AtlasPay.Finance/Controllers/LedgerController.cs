using AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;
using AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Finance.Api.Controllers;

[ApiController]
[Route("api/ledger")]
public sealed class LedgerController : ControllerBase
{
    [HttpGet("reconciliation")]
    public async Task<ActionResult<ReconcileLedgerResult>> Reconcile(
        [FromServices] ReconcileLedgerHandler handler,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        var result = await handler.HandleAsync(ct);

        return Ok(result);
    }

    [HttpGet("integrity")]
    public async Task<ActionResult<CheckLedgerIntegrityResult>> CheckIntegrity(
        [FromServices] CheckLedgerIntegrityHandler handler,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        var result = await handler.HandleAsync(ct);

        return Ok(result);
    }
}