using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class IssueCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(IssueCredentialMessage);
    }
}