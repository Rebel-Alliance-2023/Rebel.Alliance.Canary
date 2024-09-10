using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class RenewCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(RenewCredentialMessage);
    }
}