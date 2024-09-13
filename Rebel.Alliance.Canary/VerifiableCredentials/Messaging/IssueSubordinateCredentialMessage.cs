using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IssueSubordinateCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(IssueSubordinateCredentialMessage);
    }
}