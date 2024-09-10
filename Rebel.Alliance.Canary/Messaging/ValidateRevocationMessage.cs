using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class ValidateRevocationMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateRevocationMessage);
    }
}