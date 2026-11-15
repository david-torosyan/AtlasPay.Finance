using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Infrastructure.Persistence;
using AtlasPay.Finance.Infrastructure.Persistence.Repositories;
using AtlasPay.Finance.Worker;
using AtlasPay.Finance.Worker.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FinanceDatabase")));

builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
