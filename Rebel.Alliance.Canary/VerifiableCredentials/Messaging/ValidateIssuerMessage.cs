using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ValidateIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateIssuerMessage);
    }
}