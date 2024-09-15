using System;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TokenIssuerActor> _logger;

        public TokenIssuerActor(
            ICryptoService cryptoService,
            IActorMessageBus messageBus,
            IActorStateManager stateManager,
            ILogger<TokenIssuerActor> logger,
            string id) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                _logger.LogError(ex, $"Error processing message in TokenIssuerActor: {ex.Message}");
                throw;
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }
        /*
            This implementation ensures that:

            The header and payload are properly serialized to JSON.
            The JSON is converted to UTF-8 bytes.
            Those bytes are then Base64Url encoded (not standard Base64).
            The signature is created using the correctly formatted data.
            The signature is also Base64Url encoded.
            The final token is assembled in the correct format:
             encodedHeader.encodedPayload.encodedSignature        
        */
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

                var headerBytes = Encoding.UTF8.GetBytes(headerJson);
                var accessTokenPayloadBytes = Encoding.UTF8.GetBytes(accessTokenPayloadJson);
                var idTokenPayloadBytes = Encoding.UTF8.GetBytes(idTokenPayloadJson);

                var encodedHeader = Base64UrlEncode(headerBytes);
                var encodedAccessTokenPayload = Base64UrlEncode(accessTokenPayloadBytes);
                var encodedIdTokenPayload = Base64UrlEncode(idTokenPayloadBytes);

                var accessTokenDataToSign = $"{encodedHeader}.{encodedAccessTokenPayload}";
                var idTokenDataToSign = $"{encodedHeader}.{encodedIdTokenPayload}";

                var (accessTokenSignature, _) = await _cryptoService.SignDataUsingIdentifierAsync(request.ClientId, accessTokenDataToSign);
                var (idTokenSignature, _) = await _cryptoService.SignDataUsingIdentifierAsync(request.ClientId, idTokenDataToSign);

                var encodedAccessTokenSignature = Base64UrlEncode(accessTokenSignature);
                var encodedIdTokenSignature = Base64UrlEncode(idTokenSignature);

                var accessToken = $"{encodedHeader}.{encodedAccessTokenPayload}.{encodedAccessTokenSignature}";
                var idToken = $"{encodedHeader}.{encodedIdTokenPayload}.{encodedIdTokenSignature}";

                _logger.LogInformation($"Generated ID Token in TokenIssuerActor: {idToken}");

                return new TokenResponse(accessToken, idToken, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error issuing token in TokenIssuerActor");
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
                _logger.LogError(ex, $"Error handling token request: {ex.Message}");
                throw;
            }
        }

        public override Task OnActivateAsync()
        {
            _logger.LogInformation($"TokenIssuerActor {Id} activated.");
            return Task.CompletedTask;
        }
    }
}