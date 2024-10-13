using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var connectionString = "Host=localhost;Database=otus;Username=postgres;Password=postgres;Port=5432";
if (!builder.Environment.IsDevelopment())
{
    var DB_HOST = Environment.GetEnvironmentVariable("DB_HOST");
    var DB_PORT = Environment.GetEnvironmentVariable("DB_PORT");
    var DB_NAME = Environment.GetEnvironmentVariable("DB_NAME");
    var DB_USER = Environment.GetEnvironmentVariable("DB_USER");
    var DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD");
    connectionString =
        $"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}";
}

builder.Services.AddDbContext<MasterContext>(
    opt => opt.UseNpgsql(connectionString)
);

var app = builder.Build();

app.MapControllers();
Console.WriteLine("Start «Client» service");
app.Run();
