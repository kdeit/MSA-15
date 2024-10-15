using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeDAL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IBusConsumer, BusConsumer>();
builder.Services.AddSingleton<IBusProducer, BusProducer>();

var connectionString = "Host=localhost;Database=otus_order;Username=postgres;Password=postgres;Port=5432";
if (!builder.Environment.IsDevelopment())
{
    var DB_HOST = Environment.GetEnvironmentVariable("ORDER_DB_HOST");
    var DB_PORT = Environment.GetEnvironmentVariable("ORDER_DB_PORT");
    var DB_NAME = Environment.GetEnvironmentVariable("ORDER_DB_NAME");
    var DB_USER = Environment.GetEnvironmentVariable("ORDER_DB_USER");
    var DB_PASSWORD = Environment.GetEnvironmentVariable("ORDER_DB_PASSWORD");
    connectionString =
        $"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}";
}

builder.Services.AddDbContext<OrderContext>(
    opt => opt.UseNpgsql(connectionString)
);

var app = builder.Build();

// Create db
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<OrderContext>();
context.Database.EnsureCreated();
//EOF Create db

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();