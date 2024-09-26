using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


var DB_HOST = Environment.GetEnvironmentVariable("DB_HOST");
var DB_PORT = Environment.GetEnvironmentVariable("DB_PORT");
var DB_NAME = Environment.GetEnvironmentVariable("DB_NAME");
var DB_USER = Environment.GetEnvironmentVariable("DB_USER");
var DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD");

builder.Services.AddDbContext<MasterContext>(
    opt =>
        opt.UseNpgsql($"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();