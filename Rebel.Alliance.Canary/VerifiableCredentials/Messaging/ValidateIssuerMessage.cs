using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ValidateIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateIssuerMessage);
    }
}