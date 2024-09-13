using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RegisterIssuerMessage : IActorMessage
    {
        public string IssuerDid { get; }
        public string PublicKey { get; }

        public RegisterIssuerMessage(string issuerDid, string publicKey)
        {
            IssuerDid = issuerDid ?? throw new System.ArgumentNullException(nameof(issuerDid));
            PublicKey = publicKey ?? throw new System.ArgumentNullException(nameof(publicKey));
        }

        public string MessageType => nameof(RegisterIssuerMessage);
    }
}