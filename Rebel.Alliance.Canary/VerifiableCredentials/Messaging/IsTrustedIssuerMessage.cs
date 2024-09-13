using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IsTrustedIssuerMessage : IActorMessage
    {
        public string IssuerDid { get; }

        public IsTrustedIssuerMessage(string issuerDid)
        {
            IssuerDid = issuerDid ?? throw new System.ArgumentNullException(nameof(issuerDid));
        }

        public string MessageType => nameof(IsTrustedIssuerMessage);
    }
}