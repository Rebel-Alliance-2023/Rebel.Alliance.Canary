using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface IRevocationManagerActor : IActor
    {
        Task RevokeCredentialAsync(string credentialId);
        Task<bool> IsCredentialRevokedAsync(string credentialId);
        Task NotifyRevocationAsync(string credentialId);
        Task<bool> ValidateRevocationAsync(string credentialId);
    }
}
