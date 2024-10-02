using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.VerifiableCredentials;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.OIDC.Models;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor
{
    public class OIDCClientActor : ActorBase, IOIDCClientActor
    {
        private readonly IActorStateManager _stateManager;
        private readonly IActorMessageBus _messageBus;
        private readonly ILogger<OIDCClientActor> _logger;
        private VerifiableCredential _clientCredential;

        public IActorMessageBus ActorMessageBus { get; }
        public ILogger<OIDCClientActor> Logger { get; }

        public OIDCClientActor(
            string id,
            IActorStateManager stateManager,
            IActorMessageBus messageBus,
            ILogger<OIDCClientActor> logger) : base(id)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientCredential = new VerifiableCredential(); // Initialize non-nullable field
        }

        public OIDCClientActor(string? id, IActorMessageBus actorMessageBus, ILogger<OIDCClientActor> logger) : base(id)
        {
            _messageBus = actorMessageBus ?? throw new ArgumentNullException(nameof(actorMessageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientCredential = new VerifiableCredential(); // Initialize non-nullable field
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                _clientCredential = await _stateManager.GetStateAsync<VerifiableCredential>("ClientCredential")
                                   ?? new VerifiableCredential();
                _logger.LogInformation($"OIDCClientActor {Id} activated. Client credential loaded.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating OIDCClientActor: {Id}");
                throw;
            }
        }

        public async Task<string> InitiateAuthenticationAsync(string redirectUri)
        {
            try
            {
                var authorizationCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                await _stateManager.SetStateAsync("AuthorizationCode", authorizationCode);
                _logger.LogInformation($"Authentication initiated for OIDCClientActor {Id}");
                return authorizationCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating authentication for OIDCClientActor {Id}");
                throw;
            }
        }

        public async Task<TokenResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string clientId)
        {
            try
            {
                var storedCode = await _stateManager.GetStateAsync<string>("AuthorizationCode");
                if (storedCode != code)
                {
                    throw new InvalidOperationException("Invalid authorization code.");
                }

                var tokenRequest = new TokenRequestMessage(_clientCredential, redirectUri, clientId, Id);
                TokenResponse tokenResponse = await _messageBus.SendMessageAsync<ITokenIssuerActor, TokenResponse>("TokenIssuer", tokenRequest);

                _logger.LogInformation($"Authorization code exchanged for OIDCClientActor {Id}");
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exchanging authorization code for OIDCClientActor {Id}");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var validationMessage = new ValidateTokenMessage(token);
                var isValid = await _messageBus.SendMessageAsync<ICredentialVerifierActor, bool>("CredentialVerifier", validationMessage);
                _logger.LogInformation($"Token validation result for OIDCClientActor {Id}: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating token for OIDCClientActor {Id}");
                throw;
            }
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            try
            {
                switch (message)
                {
                    case InitiateAuthenticationMessage initAuthMessage:
                        return await InitiateAuthenticationAsync(initAuthMessage.RedirectUri);
                    case ExchangeAuthorizationCodeMessage exchangeCodeMessage:
                        return await ExchangeAuthorizationCodeAsync(exchangeCodeMessage.Code, exchangeCodeMessage.RedirectUri, exchangeCodeMessage.ClientId);
                    case ValidateTokenMessage validationMessage:
                        return await ValidateTokenAsync(validationMessage.Token);
                    default:
                        throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message in OIDCClientActor: {Id}");
                throw;
            }
        }
    }

}
