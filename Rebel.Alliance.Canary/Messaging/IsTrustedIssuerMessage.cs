using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class IsTrustedIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(IsTrustedIssuerMessage);
    }
}