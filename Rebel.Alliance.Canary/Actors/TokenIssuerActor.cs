using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Messaging;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;
using System.Text.Json;
using System.Text;

namespace Rebel.Alliance.Canary.Actors;

public interface ITokenIssuerActor : IActor
{
    Task<TokenResponse> IssueTokenAsync(TokenRequestMessage request);
    Task HandleTokenRequestAsync(TokenRequestMessage request);
}


public class TokenIssuerActor : ActorBase, ITokenIssuerActor
{
    private readonly ICryptoService _cryptoService;
    private readonly IActorMessageBus _messageBus;
    private readonly IActorStateManager _stateManager;

    public TokenIssuerActor(ICryptoService cryptoService, IActorMessageBus messageBus, IActorStateManager stateManager, string id)
        : base(id)
    {
        _cryptoService = cryptoService;
        _messageBus = messageBus;
        _stateManager = stateManager;
    }

    public async Task<TokenResponse> IssueTokenAsync(TokenRequestMessage request)
    {
        var payload = new TokenPayload
        {
            Issuer = request.ClientId,
            Subject = request.ClientCredential.Subject,
            IssuedAt = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddMinutes(30),
            Claims = request.ClientCredential.Claims
        };

        // Create JWT header
        var header = new Rebel.Alliance.Canary.Models.JwtHeader
        {
            Alg = "RS256",
            Typ = "JWT",
            Kid = request.ClientId
        };

        // Serialize header and payload
        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);

        // Sign the JWT
        var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(request.ClientId, $"{headerJson}.{payloadJson}");

        // Construct the JWT token
        var token = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson))}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson))}.{Convert.ToBase64String(signature)}";

        return new TokenResponse(token, payload.Expiration);
    }

    public async Task HandleTokenRequestAsync(TokenRequestMessage request)
    {
        // Handle token request message sent from other actors (e.g., OIDCClientActor)
        var tokenResponse = await IssueTokenAsync(request);
        await _messageBus.SendMessageAsync<ITokenIssuerActor, TokenResponse>(request.ReplyTo, tokenResponse);
    }

    public override Task OnActivateAsync()
    {
        // Add initialization logic here if needed
        return Task.CompletedTask;
    }
}
