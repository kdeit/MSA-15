using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeDAL;
using OtusKdeOrder.BusConsumers;
using OtusKdeTransaction;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<IBusConsumer, BusConsumer>();
builder.Services.AddScoped<IBusProducer, BusProducer>();
builder.Services.AddScoped<OrderBusConsumer>();
builder.Services.AddSingleton<OrderTransactionSaga>();

var connectionString = "Host=localhost;Database=otus_order;Username=postgres;Password=postgres;Port=5432";
if (!builder.Environment.IsDevelopment())
{
    var DB_HOST = Environment.GetEnvironmentVariable("ORDER_DB_HOST");
    var DB_PORT = Environment.GetEnvironmentVariable("ORDER_DB_PORT");
    var DB_NAME = Environment.GetEnvironmentVariable("ORDER_DB_NAME");
    var DB_USER = Environment.GetEnvironmentVariable("ORDER_DB_USER");
    var DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD");
    connectionString =
        $"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}";
}

builder.Services.AddDbContext<OrderContext>(
    opt => opt.UseNpgsql(connectionString)
);


var isProduction = Environment.GetEnvironmentVariable("DB_PASSWORD") is not null;
var HostName = isProduction
    ? "redis-master.default.svc.cluster.local"
    : "localhost";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = HostName;
    options.InstanceName = "wtm";
});
var app = builder.Build();

// Create db
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<OrderContext>();
context.Database.EnsureCreated();
//EOF Create db

services.GetService<OrderBusConsumer>().Init();

var saga = services.GetService<OrderTransactionSaga>();
saga.Handle();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();