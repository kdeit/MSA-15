using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OtusKdeDAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


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

builder.Services.AddOpenTelemetry().WithMetrics(builder =>
{
    builder.AddPrometheusExporter();
    builder.AddMeter("Microsoft.AspNetCore.Hosting",
        "Microsoft.AspNetCore.Server.Kestrel");
    builder.AddView("http.server.request.duration",
        new ExplicitBucketHistogramConfiguration
        {
            Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.Run();