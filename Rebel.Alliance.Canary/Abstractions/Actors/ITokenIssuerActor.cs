using Rebel.Alliance.Canary.Messaging;

namespace Rebel.Alliance.Canary.Abstractions.Actors;

public interface ITokenIssuerActor : IActor
{
    Task<TokenResponse> IssueTokenAsync(TokenRequestMessage request);
    Task HandleTokenRequestAsync(TokenRequestMessage request);
}
