using AtlasPay.Finance.Api.Security;
using AtlasPay.Finance.Application.Abstractions.Identity;
using AtlasPay.Finance.Application.Accounts.Commands.ActivateAccount;
using AtlasPay.Finance.Application.Accounts.Commands.CreateAccount;
using AtlasPay.Finance.Application.Accounts.Commands.FreezeAccount;
using AtlasPay.Finance.Application.Accounts.Commands.UnfreezeAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccounts;
using AtlasPay.Finance.Application.Deposits.Commands.DepositMoney;
using AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;
using AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;
using AtlasPay.Finance.Application.Transfers.Commands.ReverseTransfer;
using AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;
using AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;
using AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;
using AtlasPay.Finance.Application.Withdrawals.Commands;
using AtlasPay.Finance.Exceptions;
using AtlasPay.Finance.Infrastructure;
using AtlasPay.Finance.Infrastructure.Persistence.Seeders;
using AtlasPay.Security.Authentication;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CreateAccountHandler>();
builder.Services.AddScoped<GetAccountHandler>();
builder.Services.AddScoped<GetAccountsHandler>();
builder.Services.AddScoped<ActivateAccountHandler>();
builder.Services.AddScoped<FreezeAccountHandler>();
builder.Services.AddScoped<UnfreezeAccountHandler>();
builder.Services.AddScoped<GetAccountBalanceHandler>();
builder.Services.AddScoped<TransferMoneyHandler>();
builder.Services.AddScoped<GetTransferHandler>();
builder.Services.AddScoped<GetAccountTransactionsHandler>();
builder.Services.AddScoped<DepositMoneyHandler>();
builder.Services.AddScoped<WithdrawalMoneyHandler>();
builder.Services.AddScoped<ReverseTransferHandler>();
builder.Services.AddScoped<ReconcileLedgerHandler>();
builder.Services.AddScoped<CheckLedgerIntegrityHandler>();
builder.Services.AddScoped<ICurrentCustomer, CurrentCustomer>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAtlasPayJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("FinanceCustomer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("customer_id");
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider
        .GetRequiredService<InternalLedgerBootstrapper>();

    await bootstrapper.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.Run();
