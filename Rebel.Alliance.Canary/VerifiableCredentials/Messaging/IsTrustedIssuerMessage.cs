using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IsTrustedIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(IsTrustedIssuerMessage);
    }
}