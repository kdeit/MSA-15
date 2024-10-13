using Microsoft.AspNetCore.Mvc;

namespace OtusKde.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly HttpClient _http;
    private readonly IHostEnvironment _env;

    public AuthController(HttpClient http, IHostEnvironment env)
    {
        _http = http;
        _env = env;
        var _base = _env.IsDevelopment()
            ? new Uri("http://localhost:9090")
            : new Uri("http://keycloak.default.svc.cluster.local:9090");
        _http.BaseAddress = _base;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KeycloakTokenResponse>> GetToken([FromBody] AuthModel model)
    {
        Console.WriteLine("Get user token ...");
        Console.WriteLine($"UserName:: {model.username}");
        Console.WriteLine($"Password:: {model.password}");
        var tokenUrl = "realms/otus/protocol/openid-connect/token";
        var dict = new Dictionary<string, string>();
        dict.Add("client_id", "asptestclient");
        dict.Add("client_secret", "p8nOvrIKkAx4nfwsK0E3yP8so9hwq6Kj");
        dict.Add("grant_type", "password");
        dict.Add("username", model.username);
        dict.Add("password", model.password);
        var _ = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(dict)
        };
        var res0 = await _http.SendAsync(_);
        Console.WriteLine($"Answer:: {res0}");
        if (!res0.IsSuccessStatusCode) return BadRequest();

        return Ok(await res0.Content.ReadFromJsonAsync<KeycloakTokenResponse>());
    }

    public record AuthModel(
        string username,
        string password
    );

    public record KeycloakTokenResponse(
        string access_token,
        string token_type,
        string scope,
        int expires_in,
        int refresh_expires_in,
        int before
    );
}