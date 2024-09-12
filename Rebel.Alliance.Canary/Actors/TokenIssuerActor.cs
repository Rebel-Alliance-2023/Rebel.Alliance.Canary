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
        _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public override async Task<object> ReceiveAsync(IActorMessage message)
    {
        try
        {
            switch (message)
            {
                case TokenRequestMessage tokenRequest:
                    return await IssueTokenAsync(tokenRequest);
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message in TokenIssuerActor: {ex.Message}");
            throw;
        }
    }

    public async Task<TokenResponse> IssueTokenAsync(TokenRequestMessage request) 
    {
        try
        {
            var payload = new TokenPayload
            {
                Issuer = request.ClientId,
                Subject = request.ClientCredential.Subject,
                IssuedAt = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Claims = request.ClientCredential.Claims
            };

            var header = new Rebel.Alliance.Canary.Models.JwtHeader
            {
                Alg = "RS256",
                Typ = "JWT",
                Kid = request.ClientId
            };

            var headerJson = JsonSerializer.Serialize(header);
            var payloadJson = JsonSerializer.Serialize(payload);

            var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(request.ClientId, $"{headerJson}.{payloadJson}");

            var token = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson))}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson))}.{Convert.ToBase64String(signature)}";

            return new TokenResponse(token, payload.Expiration);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error issuing token: {ex.Message}");
            throw;
        }
    }

    public async Task HandleTokenRequestAsync(TokenRequestMessage request)
    {
        try
        {
            var tokenResponse = await IssueTokenAsync(request);
            await _messageBus.SendMessageAsync<ITokenIssuerActor, TokenResponse>(request.ReplyTo, tokenResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling token request: {ex.Message}");
            throw;
        }
    }

    public override Task OnActivateAsync()
    {
        Console.WriteLine($"TokenIssuerActor {Id} activated.");
        return Task.CompletedTask;
    }
}
