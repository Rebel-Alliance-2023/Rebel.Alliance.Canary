using Rebel.Alliance.Canary.Models;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface IVerifiableCredentialActor : IActor
    {
        Task<bool> SignCredentialAsync(VerifiableCredential credential);
        Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subjectId, DateTime expirationDate, Dictionary<string, string> claims);
    }
}