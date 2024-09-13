using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ValidateRevocationMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateRevocationMessage);
    }
}