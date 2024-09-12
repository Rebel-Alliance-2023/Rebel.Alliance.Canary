using Rebel.Alliance.Canary.Services;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface IOIDCClientActor : IActor
    {
        Task<string> InitiateAuthenticationAsync(string redirectUri);
        Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string clientId);
        Task<bool> ValidateTokenAsync(string token);
    }
}
