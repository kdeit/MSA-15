using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var connectionString = "Host=localhost;Database=otus_billing;Username=postgres;Password=postgres;Port=5432";
if (!builder.Environment.IsDevelopment())
{
    var DB_HOST = Environment.GetEnvironmentVariable("BILLING_DB_HOST");
    var DB_PORT = Environment.GetEnvironmentVariable("BILLING_DB_PORT");
    var DB_NAME = Environment.GetEnvironmentVariable("BILLING_DB_NAME");
    var DB_USER = Environment.GetEnvironmentVariable("BILLING_DB_USER");
    var DB_PASSWORD = Environment.GetEnvironmentVariable("BILLING_DB_PASSWORD");
    connectionString =
        $"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}";
}

builder.Services.AddDbContext<BillingContext>(
    opt => opt.UseNpgsql(connectionString)
);
var app = builder.Build();

// Create db
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<BillingContext>();
context.Database.EnsureCreated();
//EOF Create db

app.MapControllers();
app.Run();