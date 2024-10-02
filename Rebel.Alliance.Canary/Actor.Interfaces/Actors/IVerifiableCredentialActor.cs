using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface IVerifiableCredentialActor : IActor
    {
        Task<bool> SignCredentialAsync(VerifiableCredential credential);
        Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subjectId, DateTime expirationDate, Dictionary<string, string> claims);
    }
}