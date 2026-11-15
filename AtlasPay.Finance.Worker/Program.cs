using AtlasPay.Finance.Infrastructure.Persistence;
using AtlasPay.Finance.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FinanceDatabase")));

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
