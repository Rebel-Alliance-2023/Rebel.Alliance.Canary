using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RenewCredentialMessage : IActorMessage
    {
        public string CredentialId { get; }

        public RenewCredentialMessage(string credentialId)
        {
            CredentialId = credentialId ?? throw new System.ArgumentNullException(nameof(credentialId));
        }

        public string MessageType => nameof(RenewCredentialMessage);
    }
}
