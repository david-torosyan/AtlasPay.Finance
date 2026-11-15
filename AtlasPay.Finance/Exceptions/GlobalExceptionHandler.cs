
using AtlasPay.Finance.Application.Deposits.Exceptions;
using AtlasPay.Finance.Application.Transfers.Exceptions;
using AtlasPay.Finance.Application.Withdrawals.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Finance.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            TransferRequestConflictException =>
                StatusCodes.Status409Conflict,

            DepositRequestConflictException =>
                StatusCodes.Status409Conflict,

            WithdrawalRequestConflictException =>
                StatusCodes.Status409Conflict,

            TransferReversalConflictException =>
                StatusCodes.Status409Conflict,

            ArgumentException =>
                StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status500InternalServerError
        };

        logger.Log(
            statusCode >= 500 ? LogLevel.Error : LogLevel.Warning,
            exception,
            "Request failed with status {StatusCode}",
            statusCode);

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                400 => "Invalid request",
                409 => "Request conflict",
                _ => "Internal server error"
            },
            Detail = statusCode >= 500
                ? "An unexpected error occurred."
                : exception.Message
        };

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
    }
}
