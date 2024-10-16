using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeDAL;
using OtusKdeDAL.BusConsumers;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IBusConsumer, BusConsumer>();
builder.Services.AddScoped<BillingBusConsumer>();
builder.Services.AddHttpClient();

// Add services to the container.

builder.Services.AddControllers();
var connectionString = "Host=localhost;Database=otus_notification;Username=postgres;Password=postgres;Port=5432";
if (!builder.Environment.IsDevelopment())
{
    var DB_HOST = Environment.GetEnvironmentVariable("NOTIFICATION_DB_HOST");
    var DB_PORT = Environment.GetEnvironmentVariable("NOTIFICATION_DB_PORT");
    var DB_NAME = Environment.GetEnvironmentVariable("NOTIFICATION_DB_NAME");
    var DB_USER = Environment.GetEnvironmentVariable("NOTIFICATION_DB_USER");
    var DB_PASSWORD = Environment.GetEnvironmentVariable("NOTIFICATION_DB_PASSWORD");
    connectionString =
        $"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}";
}

builder.Services.AddDbContext<NotificationContext>(
    opt => opt.UseNpgsql(connectionString)
);

var app = builder.Build();

// Create db
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetService<NotificationContext>();
context.Database.EnsureCreated();
//EOF Create db

var consumer1 = services.GetService<BillingBusConsumer>();
consumer1.Init();

app.MapControllers();
Console.WriteLine("Start «Notification» service");
app.Run();