using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RevokeIssuerMessage : IActorMessage
    {
        public string IssuerDid { get; }

        public RevokeIssuerMessage(string issuerDid)
        {
            IssuerDid = issuerDid ?? throw new System.ArgumentNullException(nameof(issuerDid));
        }

        public string MessageType => nameof(RevokeIssuerMessage);
    }
}