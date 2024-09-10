using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class CreateRootCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(CreateRootCredentialMessage);
    }
}