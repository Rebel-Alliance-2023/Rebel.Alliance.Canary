# OIDCClientActor

## Interface

```csharp
using Dapr.Actors;
using System.Threading.Tasks;

public interface IOIDCClientActor : IActor
{
    Task<string> InitiateAuthenticationAsync(string redirectUri, string responseType, string scope, string state);
    Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string grantType);
    Task<bool> ValidateTokenAsync(string token);
}

```

## Actor

```csharp
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Microsoft.IdentityModel.Tokens;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class OIDCClientActor : Actor, IOIDCClientActor
{
    private readonly IKeyManagementService _keyManagementService;
    private readonly ICryptoService _cryptoService;
    private readonly IConfiguration _configuration;

    public OIDCClientActor(ActorHost host, IKeyManagementService keyManagementService, ICryptoService cryptoService, IConfiguration configuration)
        : base(host)
    {
        _keyManagementService = keyManagementService;
        _cryptoService = cryptoService;
        _configuration = configuration;
    }

    protected override async Task OnActivateAsync()
    {
        // Load state or initialize from VC
        var state = await StateManager.TryGetStateAsync<OIDCClientState>("OIDCClientState");
        if (!state.HasValue)
        {
            // No state exists; initialize from VC if it has been set
            var vcState = await StateManager.TryGetStateAsync<VerifiableCredential>("VerifiableCredential");
            if (vcState.HasValue)
            {
                var vc = vcState.Value;
                var clientState = new OIDCClientState
                {
                    ClientId = vc.ClientId,
                    ClientSecret = vc.ClientSecret,
                    Issuer = vc.Issuer,
                    Audience = vc.Audience
                };
                await StateManager.SetStateAsync("OIDCClientState", clientState);
            }
        }
    }

    public async Task PresentVCAsync(VerifiableCredential vc)
    {
        // Store the VC in state
        await StateManager.SetStateAsync("VerifiableCredential", vc);

        // Initialize the actor's state from the VC
        var clientState = new OIDCClientState
        {
            ClientId = vc.ClientId,
            ClientSecret = vc.ClientSecret,
            Issuer = vc.Issuer,
            Audience = vc.Audience
        };
        await StateManager.SetStateAsync("OIDCClientState", clientState);
    }

    public async Task<string> InitiateAuthenticationAsync(string redirectUri, string responseType, string scope, string state)
    {
        var clientState = await StateManager.GetStateAsync<OIDCClientState>("OIDCClientState");
        var clientId = clientState.ClientId;
        var authorizationEndpoint = $"{_configuration["Oidc:Authority"]}/authorize";

        var authRequestUri = $"{authorizationEndpoint}?client_id={clientId}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}&state={state}";
        return authRequestUri;
    }

    public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string grantType)
    {
        if (grantType != "authorization_code")
        {
            throw new InvalidOperationException("Unsupported grant type");
        }

        var clientState = await StateManager.GetStateAsync<OIDCClientState>("OIDCClientState");
        var clientId = clientState.ClientId;
        var clientSecret = clientState.ClientSecret;
        var tokenEndpoint = $"{_configuration["Oidc:Authority"]}/token";

        // Simulate token response (Replace with actual HTTP request)
        var oidcResponse = new OidcResponse
        {
            AccessToken = "mock-access-token",
            IdToken = "mock-id-token",
            TokenType = "Bearer",
            ExpiresIn = 3600 // Example: token expires in 1 hour
        };

        return oidcResponse;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var clientState = await StateManager.GetStateAsync<OIDCClientState>("OIDCClientState");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(await _keyManagementService.GetKeyAsync("JwtKey")));

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = clientState.Issuer,
                ValidAudience = clientState.Audience,
                IssuerSigningKey = key
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class OIDCClientState
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

public class VerifiableCredential
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

```