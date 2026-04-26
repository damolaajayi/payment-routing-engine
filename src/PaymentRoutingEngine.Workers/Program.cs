using PaymentRoutingEngine.Application.DependencyInjection;
using PaymentRoutingEngine.Infrastructure.DependencyInjection;
using PaymentRoutingEngine.Workers;
using PaymentRoutingEngine.Workers.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<ProcessPaymentConsumer>();
//builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
