using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface IVerifiableCredentialAsRootOfTrustActor : IActor
    {
        Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId);
        Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId);
        Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential, VerifiableCredential rootCredential);
    }
}