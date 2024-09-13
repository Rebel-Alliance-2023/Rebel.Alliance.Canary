using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IsTrustedIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(IsTrustedIssuerMessage);
    }
}