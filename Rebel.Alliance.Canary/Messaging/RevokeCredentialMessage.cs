using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class RevokeCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(RevokeCredentialMessage);
    }
}