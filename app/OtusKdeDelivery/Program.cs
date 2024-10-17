using OtusKdeBus;
using OtusKdeDelivery.BusConsumers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IBusConsumer, BusConsumer>();
builder.Services.AddSingleton<IBusProducer, BusProducer>();
builder.Services.AddSingleton<OrderBusConsumer>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var _ = services.GetService<OrderBusConsumer>();
 _.Init();
app.Run();