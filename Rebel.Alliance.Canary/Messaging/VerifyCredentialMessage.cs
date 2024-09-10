using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class VerifyCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(VerifyCredentialMessage);
    }
}