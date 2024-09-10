using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class ValidateIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(ValidateIssuerMessage);
    }
}