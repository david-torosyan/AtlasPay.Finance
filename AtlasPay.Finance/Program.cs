using AtlasPay.Finance.Application.Accounts.Commands.ActivateAccount;
using AtlasPay.Finance.Application.Accounts.Commands.CreateAccount;
using AtlasPay.Finance.Application.Accounts.Commands.FreezeAccount;
using AtlasPay.Finance.Application.Accounts.Commands.UnfreezeAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;
using AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;
using AtlasPay.Finance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CreateAccountHandler>();
builder.Services.AddScoped<GetAccountHandler>();
builder.Services.AddScoped<ActivateAccountHandler>();
builder.Services.AddScoped<FreezeAccountHandler>();
builder.Services.AddScoped<UnfreezeAccountHandler>();
builder.Services.AddScoped<GetAccountBalanceHandler>();
builder.Services.AddScoped<TransferMoneyHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
