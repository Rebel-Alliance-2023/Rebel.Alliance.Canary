using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class PresentCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(PresentCredentialMessage);
    }
}