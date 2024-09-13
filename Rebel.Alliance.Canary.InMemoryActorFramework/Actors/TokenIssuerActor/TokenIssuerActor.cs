using System;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.OIDC.Models;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TokenIssuerActor
{
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
                var issuedAt = DateTime.UtcNow;
                var expiration = issuedAt.AddMinutes(30);

                var accessTokenPayload = new TokenPayload
                {
                    Issuer = request.ClientId,
                    Subject = request.ClientCredential.Subject,
                    IssuedAt = issuedAt,
                    Expiration = expiration,
                    Claims = request.ClientCredential.Claims
                };

                var idTokenPayload = new TokenPayload
                {
                    Issuer = request.ClientId,
                    Subject = request.ClientCredential.Subject,
                    IssuedAt = issuedAt,
                    Expiration = expiration,
                    Claims = request.ClientCredential.Claims
                };

                var header = new JwtHeader
                {
                    Alg = "RS256",
                    Typ = "JWT",
                    Kid = request.ClientId
                };

                var headerJson = JsonSerializer.Serialize(header);
                var accessTokenPayloadJson = JsonSerializer.Serialize(accessTokenPayload);
                var idTokenPayloadJson = JsonSerializer.Serialize(idTokenPayload);

                var (accessTokenSignature, _) = await _cryptoService.SignDataUsingIdentifierAsync(request.ClientId, $"{headerJson}.{accessTokenPayloadJson}");
                var (idTokenSignature, _) = await _cryptoService.SignDataUsingIdentifierAsync(request.ClientId, $"{headerJson}.{idTokenPayloadJson}");

                var accessToken = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson))}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(accessTokenPayloadJson))}.{Convert.ToBase64String(accessTokenSignature)}";
                var idToken = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson))}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(idTokenPayloadJson))}.{Convert.ToBase64String(idTokenSignature)}";

                return new TokenResponse(accessToken, idToken, expiration);
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
}
