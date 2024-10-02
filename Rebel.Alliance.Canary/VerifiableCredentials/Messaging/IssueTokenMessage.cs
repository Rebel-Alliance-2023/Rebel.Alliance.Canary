using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{

    public class IssueTokenMessage : IActorMessage
    {
        public string MessageType => nameof(IssueTokenMessage);
    }
}