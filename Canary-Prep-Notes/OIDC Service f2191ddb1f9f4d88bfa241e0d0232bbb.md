# OIDC Service

## Models

```csharp
public class AuthorizationCode
{
    public string Code { get; set; }
    public string RedirectUri { get; set; }
    public string ClientId { get; set; }
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } // Add UserId to link to the user
}

public class AccessToken
{
    public string Token { get; set; }
    public string ClientId { get; set; }
    public string UserId { get; set; }
    public DateTime Expiration { get; set; }
}

public class IdToken
{
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime Expiration { get; set; }
}

public class OidcResponse
{
    public string AccessToken { get; set; }
    public string IdToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
}

```

## Service

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.Extensions.Configuration;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class OidcProviderService
{
    private readonly ConcurrentDictionary<string, AuthorizationCode> _authorizationCodes = new();
    private readonly ConcurrentDictionary<string, AccessToken> _accessTokens = new();
    private readonly ConcurrentDictionary<string, IdToken> _idTokens = new();
    private readonly ICryptoService _cryptoService;
    private readonly IConfiguration _configuration;

    public OidcProviderService(ICryptoService cryptoService, IConfiguration configuration)
    {
        _cryptoService = cryptoService;
        _configuration = configuration;
    }

    private ITokenIssuerActor GetTokenIssuerActor(string actorId)
    {
        var actorIdObj = new ActorId(actorId);
        return ActorProxy.Create<ITokenIssuerActor>(actorIdObj, nameof(TokenIssuerActor));
    }

    private IOIDCClientActor GetOIDCClientActor(string actorId)
    {
        var actorIdObj = new ActorId(actorId);
        return ActorProxy.Create<IOIDCClientActor>(actorIdObj, nameof(OIDCClientActor));
    }

    public async Task<string> GenerateAuthorizationCodeAsync(string clientId, string redirectUri, string userId)
    {
        var code = Convert.ToBase64String(_cryptoService.GenerateRandomBytes(32));
        var authorizationCode = new AuthorizationCode
        {
            Code = code,
            ClientId = clientId,
            RedirectUri = redirectUri,
            Expiration = DateTime.UtcNow.AddMinutes(5),
            UserId = userId
        };
        _authorizationCodes[code] = authorizationCode;
        return code;
    }

    public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string clientId, string redirectUri)
    {
        if (!_authorizationCodes.TryGetValue(code, out var authorizationCode) ||
            authorizationCode.ClientId != clientId ||
            authorizationCode.RedirectUri != redirectUri ||
            authorizationCode.Expiration < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invalid authorization code");
        }

        _authorizationCodes.TryRemove(code, out _);

        var userId = authorizationCode.UserId;
        var tokenIssuer = GetTokenIssuerActor(userId);
        var accessToken = await tokenIssuer.IssueTokenAsync(userId, new Dictionary<string, string>
        {
            { "client_id", clientId }
        });

        var idToken = await GenerateIdToken(userId);

        return new OidcResponse
        {
            AccessToken = accessToken,
            IdToken = idToken.Token,
            TokenType = "Bearer",
            ExpiresIn = (int)(DateTime.UtcNow.AddHours(1) - DateTime.UtcNow).TotalSeconds
        };
    }

    private async Task<IdToken> GenerateIdToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var userClaims = await GetUserClaimsAsync(userId);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(userClaims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var idToken = new IdToken
        {
            Token = tokenHandler.WriteToken(token),
            UserId = userId,
            Expiration = tokenDescriptor.Expires.Value
        };
        _idTokens[idToken.Token] = idToken;
        return idToken;
    }

    private async Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId)
    {
        var credentialIssuer = GetOIDCClientActor(userId);
        var credential = await credentialIssuer.InitiateAuthenticationAsync(userId, "code", "openid", "state");
        return new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim("vc", credential) // Include VC in the claims
        };
    }

    public async Task<Dictionary<string, string>> GetUserInfoAsync(string accessToken)
    {
        var tokenIssuer = GetTokenIssuerActor("token-issuer");
        if (!await tokenIssuer.ValidateTokenAsync(accessToken))
        {
            throw new InvalidOperationException("Invalid access token");
        }

        // Fetch user info based on the token
        var userInfo = new Dictionary<string, string>
        {
            { "sub", "user-id" },
            { "name", "User Name" },
            { "email", "user@example.com" }
        };
        return userInfo;
    }
}

```

## OidcController

```csharp
using Microsoft.AspNetCore.Mvc;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("oidc")]
[ApiController]
public class OidcController : ControllerBase
{
    private readonly OidcProviderService _oidcProviderService;

    public OidcController(OidcProviderService oidcProviderService)
    {
        _oidcProviderService = oidcProviderService;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery] string clientId, [FromQuery] string redirectUri, [FromQuery] string responseType, [FromQuery] string scope, [FromQuery] string state)
    {
        var authorizationCode = await _oidcProviderService.GenerateAuthorizationCodeAsync(clientId, redirectUri, User.Identity.Name);
        return Redirect($"{redirectUri}?code={authorizationCode}&state={state}");
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromForm] string clientId, [FromForm] string clientSecret, [FromForm] string code, [FromForm] string redirectUri, [FromForm] string grantType)
    {
        if (grantType != "authorization_code")
        {
            return BadRequest("Unsupported grant type");
        }

        var oidcResponse = await _oidcProviderService.ExchangeAuthorizationCodeAsync(code, clientId, redirectUri);
        return Ok(oidcResponse);
    }

    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userInfo = await _oidcProviderService.GetUserInfoAsync(accessToken);
        return Ok(userInfo);
    }
}

```

## **Configure OIDC Middleware**

### **Program.cs**

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Dapr.Actors;
using Dapr.Actors.AspNetCore;
using Dapr.Actors.Runtime;
using SecureMessagingApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Oidc:Authority"];
    options.ClientId = builder.Configuration["Oidc:ClientId"];
    options.ClientSecret = builder.Configuration["Oidc:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            // Custom token validation logic
            var oidcProviderService = context.HttpContext.RequestServices.GetRequiredService<OidcProviderService>();
            var token = context.SecurityToken as JwtSecurityToken;
            if (token != null)
            {
                var isValid = await oidcProviderService.ValidateTokenAsync(token.RawData);
                if (!isValid)
                {
                    context.Fail("Invalid token");
                }
            }
        },
        OnUserInformationReceived = context =>
        {
            // Custom user information retrieval logic
            return Task.CompletedTask;
        },
        OnAccessDenied = context =>
        {
            // Custom access-denied handling
            return Task.CompletedTask;
        }
    };
});

// Register Canary services
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IKeyManagementService, KeyManagementService>();
builder.Services.AddSingleton<OidcProviderService>();

// Register Dapr actors
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<CredentialIssuerActor>();
    options.Actors.RegisterActor<CredentialVerifierActor>();
    options.Actors.RegisterActor<VerifiableCredentialActor>();
    options.Actors.RegisterActor<CredentialHolderActor>();
    options.Actors.RegisterActor<RevocationManagerActor>();
    options.Actors.RegisterActor<TrustFrameworkManagerActor>();
    options.Actors.RegisterActor<VerifiableCredentialAsRootOfTrustActor>();
    options.Actors.RegisterActor<TokenIssuerActor>();
    options.Actors.RegisterActor<OIDCClientActor>(); // Register the new OIDCClientActor
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapActorsHandlers();
});

app.Run();

```