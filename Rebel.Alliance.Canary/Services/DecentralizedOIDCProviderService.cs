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
            // Create an InitiateAuthenticationMessage
            var message = new InitiateAuthenticationMessage(clientId, redirectUri);

            // Send the message to the OIDCClientActor
            var authorizationCode = await _messageBus.SendMessageAsync<OIDCClientActor, string>(clientId, message);

            // Construct the redirect URL with the authorization code
            var redirectUrl = $"{redirectUri}?code={authorizationCode}";

            return redirectUrl;
        }


        public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string clientId, string code, string redirectUri)
        {
            var message = new ExchangeAuthorizationCodeMessage(code, redirectUri, clientId);
            return await _messageBus.SendMessageAsync<OIDCClientActor, OidcResponse>(clientId, message);
        }


    }

}

