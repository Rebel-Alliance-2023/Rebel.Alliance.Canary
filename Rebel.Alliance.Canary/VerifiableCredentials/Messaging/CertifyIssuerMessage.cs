using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class CertifyIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(CertifyIssuerMessage);
    }
}