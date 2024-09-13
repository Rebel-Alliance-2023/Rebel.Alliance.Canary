using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IssueCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(IssueCredentialMessage);
    }
}