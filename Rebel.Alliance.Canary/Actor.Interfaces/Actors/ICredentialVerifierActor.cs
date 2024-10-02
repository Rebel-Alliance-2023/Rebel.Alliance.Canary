using Rebel.Alliance.Canary.VerifiableCredentials;
using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface ICredentialVerifierActor : IActor
    {
        Task<bool> VerifyCredentialAsync(VerifiableCredential credential);
        Task<bool> ValidateTokenAsync(string token);
    }
}
