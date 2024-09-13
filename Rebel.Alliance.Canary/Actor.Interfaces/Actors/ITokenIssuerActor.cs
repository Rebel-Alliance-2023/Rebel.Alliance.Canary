using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.OIDC.Models;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors;

public interface ITokenIssuerActor : IActor
{
    Task<TokenResponse> IssueTokenAsync(TokenRequestMessage request);
    Task HandleTokenRequestAsync(TokenRequestMessage request);
}
