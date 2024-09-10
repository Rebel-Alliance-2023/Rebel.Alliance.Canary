using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class UpdateRegistryMessage : IActorMessage
    {
        public string MessageType => nameof(UpdateRegistryMessage);
    }
}