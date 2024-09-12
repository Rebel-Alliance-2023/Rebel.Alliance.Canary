using Rebel.Alliance.Canary.Models;

namespace Rebel.Alliance.Canary.Abstractions.Actors
{
    public interface ICredentialIssuerActor : IActor
    {
        Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subject, Dictionary<string, string> claims);
    }
}