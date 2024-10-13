using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

namespace OtusKdeClient.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : Controller
{
    private readonly MasterContext _cnt;
    private readonly HttpClient _http;
    private readonly IHostEnvironment _env;

    public UserController(MasterContext context, HttpClient http, IHostEnvironment env)
    {
        _cnt = context;
        _http = http;
        _env = env;
        
        _http.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:9090")
            : new Uri("http://keycloak.default.svc.cluster.local:9090");
    }

    [HttpGet("{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetByEmail(string email)
    {
        var user = await GetUser(email);
        
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(UserCreateUpdateRequest model)
    {
        Console.WriteLine("Create user");
        await SetToken();
        JsonContent resp1 = JsonContent.Create(new KeycloakUserCreate(model.Email, true));
        var res1 = await _http.PostAsync($"admin/realms/otus/users", resp1);

        if (res1.IsSuccessStatusCode)
        {
            await SetPassword(model.Email, model.Password);

            User newUser = new User()
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            _cnt.Add(newUser);
            await _cnt.SaveChangesAsync();
        }

        return res1.IsSuccessStatusCode ? Ok() : BadRequest();
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(UserCreateUpdateRequest model)
    {
        var user = await GetUser(model.Email);
        if (user is null) return BadRequest();
        
        if (model.Password is not null && model.Password != "")
        {
            await SetToken();
            await SetPassword(model.Email, model.Password);
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        await _cnt.SaveChangesAsync();
        
        return Ok();
    }

    private async Task<User> GetUser(string email)
    {
        return await _cnt.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    private async Task SetToken()
    {
        Console.WriteLine("Get token ...");
        var tokenUrl = "realms/otus/protocol/openid-connect/token";
        var dict = new Dictionary<string, string>();
        dict.Add("client_id", "asptestclient");
        dict.Add("client_secret", "p8nOvrIKkAx4nfwsK0E3yP8so9hwq6Kj");
        dict.Add("grant_type", "client_credentials");
        var _ = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(dict)
        };
        var res0 = await _http.SendAsync(_);
        var _token = await res0.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
        _http.DefaultRequestHeaders.Add("Authorization", $"{_token.token_type} {_token.access_token}");
        Console.WriteLine($"Set token {_token}");
    }

    private async Task<bool> SetPassword(string email, string password)
    {
        var res2 = await _http.GetAsync($"admin/realms/otus/users?email={email}");
        var ans2 = await res2.Content.ReadFromJsonAsync<IEnumerable<KeycloakUserResponse>>();
        if (ans2.Count() != 1) return false;

        JsonContent resp3 = JsonContent.Create(new KeycloakChangePasswordValues(password));
        var res3 =
            await _http.PutAsync($"admin/realms/otus/users/{ans2.FirstOrDefault().id}/reset-password", resp3);
        if (!res3.IsSuccessStatusCode) return false;

        return true;
    }

    public record KeycloakUserCreate(string email, bool enabled);

    public record KeycloakFindFilter(string email);

    public record KeycloakChangePasswordValues(string value);

    public record KeycloakTokenResponse(
        string access_token,
        string token_type,
        string scope,
        int expires_in,
        int refresh_expires_in,
        int before
    );

    public record KeycloakUserResponse(
        string id,
        string username,
        string email,
        bool emailVerified,
        bool enabled);
}