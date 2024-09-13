using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{

    public class IssueTokenMessage : IActorMessage
    {
        public string MessageType => nameof(IssueTokenMessage);
    }
}