using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface ICredentialVerifierActor : IActor
    {
        Task<bool> VerifyCredentialAsync(VerifiableCredential credential);
    }
}