using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class IssueSubordinateCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(IssueSubordinateCredentialMessage);
    }
}