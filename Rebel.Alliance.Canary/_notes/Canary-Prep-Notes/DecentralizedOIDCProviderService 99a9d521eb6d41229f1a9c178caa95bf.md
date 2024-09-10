# DecentralizedOIDCProviderService

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

public class DecentralizedOIDCProviderService
{
    private readonly ConcurrentDictionary<string, AuthorizationCode> _authorizationCodes = new();
    private readonly ICryptoService _cryptoService;
    private readonly IKeyManagementService _keyManagementService;
    private readonly IConfiguration _configuration;

    public DecentralizedOIDCProviderService(ICryptoService cryptoService, IKeyManagementService keyManagementService, IConfiguration configuration)
    {
        _cryptoService = cryptoService;
        _keyManagementService = keyManagementService;
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

    private async Task<VerifiableCredential> RetrieveVCAsync(string clientId)
    {
        // Simulate retrieval of VC based on clientId
        // In practice, this would involve querying a decentralized storage or an external service
        return new VerifiableCredential
        {
            ClientId = clientId,
            ClientSecret = "your-client-secret",
            Issuer = "https://your-issuer.com",
            Audience = "your-audience"
        };
    }

    private async Task PresentVCToActorAsync<T>(IActor actor, VerifiableCredential vc) where T : IActor
    {
        if (typeof(T) == typeof(ITokenIssuerActor))
        {
            var tokenIssuerActor = actor as ITokenIssuerActor;
            await tokenIssuerActor.PresentVCAsync(vc);
        }
        else if (typeof(T) == typeof(IOIDCClientActor))
        {
            var oidcClientActor = actor as IOIDCClientActor;
            await oidcClientActor.PresentVCAsync(vc);
        }
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

        // Retrieve and present VC to OIDCClientActor
        var vc = await RetrieveVCAsync(clientId);
        var oidcClientActor = GetOIDCClientActor(clientId);
        await PresentVCToActorAsync<IOIDCClientActor>(oidcClientActor, vc);

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

        // Retrieve and present VC to TokenIssuerActor
        var vc = await RetrieveVCAsync(clientId);
        var tokenIssuerActor = GetTokenIssuerActor(clientId);
        await PresentVCToActorAsync<ITokenIssuerActor>(tokenIssuerActor, vc);

        var claims = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "user_id", userId }
        };
        var accessToken = await tokenIssuerActor.IssueTokenAsync(userId, claims);

        var oidcClientActor = GetOIDCClientActor(clientId);
        var idToken = await oidcClientActor.ExchangeAuthorizationCodeAsync(code, redirectUri, "authorization_code");

        return new OidcResponse
        {
            AccessToken = accessToken,
            IdToken = idToken.IdToken,
            TokenType = "Bearer",
            ExpiresIn = (int)(DateTime.UtcNow.AddHours(1) - DateTime.UtcNow).TotalSeconds
        };
    }

    public async Task<Dictionary<string, string>> GetUserInfoAsync(string accessToken)
    {
        var tokenIssuerActor = GetTokenIssuerActor("token-issuer");
        if (!await tokenIssuerActor.ValidateTokenAsync(accessToken))
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