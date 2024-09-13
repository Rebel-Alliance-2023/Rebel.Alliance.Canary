using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RevokeIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(RevokeIssuerMessage);
    }
}