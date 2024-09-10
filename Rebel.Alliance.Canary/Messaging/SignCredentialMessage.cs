using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class SignCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(SignCredentialMessage);
    }
}