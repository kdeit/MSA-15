using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services
    .AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://localhost:9090/realms/master";
        options.ClientId = "asptestclient";
        options.ClientSecret = "FErJwOXeuonP7UUh5DOFcf8xLuvXpR61";
        
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.CallbackPath = "/login-callback"; // Update callback path
        options.SignedOutCallbackPath = "/logout-callback"; // Update signout callback path
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            NameClaimType = "email",
            RoleClaimType = "roles"
        };
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });



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

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.Run();