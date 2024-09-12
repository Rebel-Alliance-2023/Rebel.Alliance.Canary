using Rebel.Alliance.Canary.Models;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface ICredentialHolderActor : IActor
    {
        Task StoreCredentialAsync(VerifiableCredential credential);
        Task<VerifiableCredential> PresentCredentialAsync(string credentialId);
        Task RenewCredentialAsync(string credentialId);
    }
}
