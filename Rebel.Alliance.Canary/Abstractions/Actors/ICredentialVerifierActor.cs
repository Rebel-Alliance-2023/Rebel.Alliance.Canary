using Rebel.Alliance.Canary.Models;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface ICredentialVerifierActor : IActor
    {
        Task<bool> VerifyCredentialAsync(VerifiableCredential credential);
    }
}