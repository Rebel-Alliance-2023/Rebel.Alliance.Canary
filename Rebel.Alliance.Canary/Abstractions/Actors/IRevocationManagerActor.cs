namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface IRevocationManagerActor : IActor
    {
        Task RevokeCredentialAsync(string credentialId);
        Task<bool> IsCredentialRevokedAsync(string credentialId);
        Task NotifyRevocationAsync(string credentialId);
        Task<bool> ValidateRevocationAsync(string credentialId);
    }
}
