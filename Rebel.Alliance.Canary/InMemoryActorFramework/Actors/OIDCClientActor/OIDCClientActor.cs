using MediatR;
using Rebel.Alliance.Canary.Messaging;
using System;
using System.Threading.Tasks;
using Rebel.Alliance.Canary.InMemoryActorFramework;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Abstractions.Actors;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor
{

    public class OIDCClientActor : ActorBase, IOIDCClientActor
    {
        private readonly IActorStateManager _stateManager;
        private readonly IActorMessageBus _messageBus;
        private VerifiableCredential _clientCredential;

        public OIDCClientActor(string id, IActorStateManager stateManager, IActorMessageBus messageBus) : base(id)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                _clientCredential = await _stateManager.GetStateAsync<VerifiableCredential>("ClientCredential")
                                   ?? new VerifiableCredential();
                Console.WriteLine($"OIDCClientActor {Id} activated. Client credential loaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error activating OIDCClientActor: {ex.Message}");
                throw;
            }
        }

        public async Task<string> InitiateAuthenticationAsync(string redirectUri)
        {
            try
            {
                var authorizationCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                await _stateManager.SetStateAsync("AuthorizationCode", authorizationCode);
                Console.WriteLine($"Authentication initiated for OIDCClientActor {Id}");
                return authorizationCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating authentication: {ex.Message}");
                throw;
            }
        }

        public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string clientId)
        {
            try
            {
                var storedCode = await _stateManager.GetStateAsync<string>("AuthorizationCode");
                if (storedCode != code)
                {
                    throw new InvalidOperationException("Invalid authorization code.");
                }

                var tokenRequest = new TokenRequestMessage(_clientCredential, redirectUri, clientId, Id);
                var oidcResponse = await _messageBus.SendMessageAsync<ITokenIssuerActor, OidcResponse>("TokenIssuer", tokenRequest);

                Console.WriteLine($"Authorization code exchanged for OIDCClientActor {Id}");
                return oidcResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exchanging authorization code: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var validationMessage = new TokenValidationMessage(token, "expectedIssuer", "expectedAudience", "clientId", 60);
                var isValid = await _messageBus.SendMessageAsync<ICredentialVerifierActor, bool>("CredentialVerifier", validationMessage);
                Console.WriteLine($"Token validation result for OIDCClientActor {Id}: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
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
                    case TokenValidationMessage validationMessage:
                        return await ValidateTokenAsync(validationMessage.Token);
                    default:
                        throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message in OIDCClientActor: {ex.Message}");
                throw;
            }
        }
    }
}
