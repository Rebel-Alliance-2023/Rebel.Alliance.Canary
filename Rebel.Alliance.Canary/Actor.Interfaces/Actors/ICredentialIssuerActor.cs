using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.Actor.Interfaces.Actors
{
    public interface ICredentialIssuerActor : IActor
    {
        Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subject, Dictionary<string, string> claims);
    }
}