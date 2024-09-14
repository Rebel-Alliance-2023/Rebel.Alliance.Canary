using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.OIDC.Models;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TokenIssuerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.RevocationManagerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor.Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;

namespace Rebel.Alliance.Canary.OIDC.Services
{
    public interface IDecentralizedOIDCProviderService
    {
        Task<AuthorizationResponse> InitiateAuthenticationAsync(AuthenticationRequest request);
        Task<TokenResponse> ExchangeAuthorizationCodeAsync(TokenRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task RevokeCredentialAsync(string credentialId);

        Task<OidcResponse> ExchangeAuthorizationCodeAsync(string clientId, string code, string redirectUri);
        Task<string> InitiateAuthenticationAsync(string clientId, string redirectUri);
        Task GenerateAndStorePrivateKeyAsync(string clientId);
    }

    public class DecentralizedOIDCProvider : IDecentralizedOIDCProviderService
    {
        private readonly IActorMessageBus _actorMessageBus;
        private readonly ICryptoService _cryptoService;
        private readonly IKeyStore _keyStore;

        public DecentralizedOIDCProvider(
            IActorMessageBus actorMessageBus,
            ICryptoService cryptoService,
            IKeyStore keyStore)
        {
            _actorMessageBus = actorMessageBus;
            _cryptoService = cryptoService;
            _keyStore = keyStore;
        }

        public async Task<string> InitiateAuthenticationAsync(string clientId, string redirectUri)
        {
            // Create an InitiateAuthenticationMessage
            var message = new InitiateAuthenticationMessage(clientId, redirectUri);

            // Send the message to the OIDCClientActor
            var authorizationCode = await _actorMessageBus.SendMessageAsync<OIDCClientActor, string>(clientId, message);

            // Construct the redirect URL with the authorization code
            var redirectUrl = $"{redirectUri}?code={authorizationCode}";

            return redirectUrl;
        }

        public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string clientId, string code, string redirectUri)
        {
            var message = new ExchangeAuthorizationCodeMessage(code, redirectUri, clientId);
            return await _actorMessageBus.SendMessageAsync<OIDCClientActor, OidcResponse>(clientId, message);
        }

        public async Task<AuthorizationResponse> InitiateAuthenticationAsync(AuthenticationRequest request)
        {
            // Use _actorMessageBus to send a message to OIDCClientActor
            var response = await _actorMessageBus.SendMessageAsync<OIDCClientActor, AuthorizationResponse>(
                "OIDCClient",
                new InitiateAuthenticationMessage(request.ClientId, request.RedirectUri));
            return response;
        }

        public async Task<TokenResponse> ExchangeAuthorizationCodeAsync(TokenRequest request)
        {
            // Use _actorMessageBus to send a message to TokenIssuerActor
            var response = await _actorMessageBus.SendMessageAsync<TokenIssuerActor, TokenResponse>(
                "TokenIssuer",
                new ExchangeAuthorizationCodeMessage(request.Code, request.RedirectUri, request.ClientId));
            return response;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var isValid = await _actorMessageBus.SendMessageAsync<CredentialVerifierActor, bool>(
                "CredentialVerifier",
                new ValidateTokenMessage(token));
            return isValid;
        }

        public async Task RevokeCredentialAsync(string credentialId)
        {
            // Use _actorMessageBus to send a message to RevocationManagerActor
            await _actorMessageBus.SendMessageAsync<RevocationManagerActor>(
                "RevocationManager",
                new RevokeCredentialMessage(credentialId));
        }

        public async Task GenerateAndStorePrivateKeyAsync(string clientId)
        {
            // Generate a new key pair
            var (publicKey, privateKey) = await _cryptoService.GenerateKeyPairAsync();

            // Store the private key using the IKeyStore
            await _keyStore.StoreKeyAsync(clientId, privateKey);
        }
    }
}