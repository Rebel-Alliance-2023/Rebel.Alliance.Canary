using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class ValidateTokenMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateTokenMessage);
    }
}