using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class StoreCredentialMessage : IActorMessage
    {
        public VerifiableCredential VerifiableCredential { get; }

        public StoreCredentialMessage(VerifiableCredential verifiableCredential)
        {
            VerifiableCredential = verifiableCredential ?? throw new System.ArgumentNullException(nameof(verifiableCredential));
        }

        public string MessageType => nameof(StoreCredentialMessage);
    }
}