using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface ICredentialHolderActor : IActor
    {
        Task StoreCredentialAsync(VerifiableCredential credential);
        Task<VerifiableCredential> PresentCredentialAsync(string credentialId);
        Task RenewCredentialAsync(string credentialId);
    }
}
