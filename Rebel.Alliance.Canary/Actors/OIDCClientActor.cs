using MediatR;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Messaging;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;
using System;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Actors
{
    public interface IOIDCClientActor : IActor
    {
        Task<string> InitiateAuthenticationAsync(string redirectUri);
        Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri);
        Task<bool> ValidateTokenAsync(string token);
    }

    public class OIDCClientActor : ActorBase, IOIDCClientActor
    {
        private readonly IActorStateManager _stateManager;
        private readonly IActorMessageBus _messageBus;
        private VerifiableCredential _clientCredential;

        public OIDCClientActor(string id, IActorStateManager stateManager, IActorMessageBus messageBus) : base(id)
        {
            _stateManager = stateManager;
            _messageBus = messageBus;
        }

        public override async Task OnActivateAsync()
        {
            // Load or initialize client credential from state
            _clientCredential = await _stateManager.GetStateAsync<VerifiableCredential>("ClientCredential")
                               ?? new VerifiableCredential();

            // Register handlers for OIDC events or messages
            await _messageBus.RegisterHandlerAsync<OidcRequest>(HandleOidcRequestAsync);
        }

        public async Task<string> InitiateAuthenticationAsync(string redirectUri)
        {
            // Generate an authorization code
            var authorizationCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            await _stateManager.SetStateAsync("AuthorizationCode", authorizationCode);

            // Return authorization code to redirect the user to the OIDC provider
            return authorizationCode;
        }

        public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri)
        {
            // Verify the authorization code
            var storedCode = await _stateManager.GetStateAsync<string>("AuthorizationCode");
            if (storedCode != code)
            {
                throw new InvalidOperationException("Invalid authorization code.");
            }

            // Send a request to TokenIssuerActor to generate tokens
            var replyTo = this.Id; // Assuming 'this.Id' represents the current actor's unique identifier
            var tokenRequest = new TokenRequestMessage(_clientCredential, redirectUri, "client-id", replyTo);
            var oidcResponse = await _messageBus.SendMessageAsync<TokenIssuerActor, OidcResponse>("TokenIssuer", tokenRequest);

            return oidcResponse;
        }


        public async Task<bool> ValidateTokenAsync(string token)
        {
            var validationMessage = new TokenValidationMessage(token, "expectedIssuer", "expectedAudience", "clientId", 60);
            var isValid = await _messageBus.SendMessageAsync<CredentialVerifierActor, bool>("CredentialVerifier", validationMessage);
            return isValid;
        }

        private async Task HandleOidcRequestAsync(OidcRequest request)
        {
            // Handle incoming OIDC requests (e.g., token issuance, validation)
        }
    }
}
