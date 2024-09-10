using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class CertifyIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(CertifyIssuerMessage);
    }
}