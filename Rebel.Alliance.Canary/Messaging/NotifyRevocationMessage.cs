using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class NotifyRevocationMessage : IActorMessage
    {
        public string MessageType => nameof(NotifyRevocationMessage);
    }
}