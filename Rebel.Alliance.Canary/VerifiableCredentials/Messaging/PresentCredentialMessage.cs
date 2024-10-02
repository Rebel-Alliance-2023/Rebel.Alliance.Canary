using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class PresentCredentialMessage : IActorMessage
    {
        public string CredentialId { get; }

        public PresentCredentialMessage(string credentialId)
        {
            CredentialId = credentialId ?? throw new System.ArgumentNullException(nameof(credentialId));
        }

        public string MessageType => nameof(PresentCredentialMessage);
    }
}
