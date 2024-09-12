using Rebel.Alliance.Canary.Models;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface IVerifiableCredentialAsRootOfTrustActor : IActor
    {
        Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId);
        Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId);
        Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential, VerifiableCredential rootCredential);
    }
}