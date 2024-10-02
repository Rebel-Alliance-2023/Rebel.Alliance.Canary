using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface ICredentialHolderActor : IActor
    {
        Task StoreCredentialAsync(VerifiableCredential credential);
        Task<VerifiableCredential> PresentCredentialAsync(string credentialId);
        Task RenewCredentialAsync(string credentialId);
    }
}
