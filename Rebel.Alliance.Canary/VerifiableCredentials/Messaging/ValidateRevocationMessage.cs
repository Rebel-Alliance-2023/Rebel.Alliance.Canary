using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ValidateRevocationMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateRevocationMessage);
    }
}