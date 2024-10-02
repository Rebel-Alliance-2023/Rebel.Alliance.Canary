using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.OIDC.Models;
using Rebel.Alliance.Canary.OIDC.Services;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface IOIDCClientActor : IActor
    {
        Task<string> InitiateAuthenticationAsync(string redirectUri);
        Task<TokenResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string clientId);
        Task<bool> ValidateTokenAsync(string token);
    }
}
