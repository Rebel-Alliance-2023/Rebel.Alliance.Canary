using Rebel.Alliance.Canary.Actors;
using Rebel.Alliance.Canary.Messaging;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Services
{
    public class DecentralizedOIDCProviderService
    {
        private readonly IActorMessageBus _messageBus;

        public DecentralizedOIDCProviderService(IActorMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public async Task<string> InitiateAuthenticationAsync(string clientId, string redirectUri)
        {
            // Send message to OIDCClientActor to initiate authentication flow
            var oidcClientActor = await _messageBus.SendMessageAsync<OIDCClientActor, string>(clientId, new InitiateAuthenticationMessage(clientId, redirectUri));
            return oidcClientActor;
        }

        public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string clientId, string code, string redirectUri)
        {
            // Send message to OIDCClientActor to exchange authorization code for tokens
            var oidcClientActor = await _messageBus.SendMessageAsync<OIDCClientActor, OidcResponse>(clientId, new ExchangeAuthorizationCodeMessage(code, redirectUri));
            return oidcClientActor;
        }

        public async Task<bool> ValidateTokenAsync(string clientId, string token)
        {
            // Send message to OIDCClientActor to validate the token
            var oidcClientActor = await _messageBus.SendMessageAsync<OIDCClientActor, bool>(clientId, new ValidateTokenMessage(clientId, token));
            return oidcClientActor;
        }
    }

}

