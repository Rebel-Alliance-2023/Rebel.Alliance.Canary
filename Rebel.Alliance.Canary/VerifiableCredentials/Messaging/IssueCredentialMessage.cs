using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IssueCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(IssueCredentialMessage);
    }
}