using Bogus;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

var builder = WebApplication.CreateBuilder(args);

var DB_HOST = Environment.GetEnvironmentVariable("DB_HOST");
var DB_PORT = Environment.GetEnvironmentVariable("DB_PORT");
var DB_NAME = Environment.GetEnvironmentVariable("DB_NAME");
var DB_USER = Environment.GetEnvironmentVariable("DB_USER");
var DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD");

//Console.Write($"DB params: {DB_HOST} | {DB_PORT} | {DB_NAME} | {DB_USER} | {DB_PASSWORD} ");

builder.Services.AddDbContext<MasterContext>(
    opt => 
        opt.UseNpgsql($"Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}")
);

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<MasterContext>();
context.Database.EnsureDeleted();
context.Database.EnsureCreated();

var faker = new Faker<User>()
    .RuleFor(u => u.Name, (f) => f.Name.FullName())
    .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
    .FinishWith((f, u) =>
{
    Console.WriteLine("User Created! Email={0}", u.Email);
});

context.AddRange(faker.Generate(100));
await context.SaveChangesAsync();

Console.Write("Task complete");
