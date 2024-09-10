using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{

    public class IssueTokenMessage : IActorMessage
    {
        public string MessageType => nameof(IssueTokenMessage);
    }
}