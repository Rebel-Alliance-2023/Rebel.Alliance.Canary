using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    // Message classes
    public class CreateCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(CreateCredentialMessage);
    }
}