using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RevokeIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(RevokeIssuerMessage);
    }
}