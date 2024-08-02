using Bogus;
using Microsoft.EntityFrameworkCore;
using OtusKde.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MasterContext>(
    opt => 
        opt.UseNpgsql("Host=localhost;Database=kdeOtusDb;Username=postgres;Password=docker;Port=5432")
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
