using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class CertifyIssuerMessage : IActorMessage
    {
        public string IssuerDid { get; }

        public CertifyIssuerMessage(string issuerDid)
        {
            IssuerDid = issuerDid ?? throw new System.ArgumentNullException(nameof(issuerDid));
        }

        public string MessageType => nameof(CertifyIssuerMessage);
    }
}