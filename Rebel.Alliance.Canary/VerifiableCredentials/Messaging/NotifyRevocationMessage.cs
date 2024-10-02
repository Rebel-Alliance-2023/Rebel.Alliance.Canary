using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class NotifyRevocationMessage : IActorMessage
    {
        public string CredentialId { get; }

        public NotifyRevocationMessage(string credentialId)
        {
            CredentialId = credentialId ?? throw new System.ArgumentNullException(nameof(credentialId));
        }

        public string MessageType => nameof(NotifyRevocationMessage);
    }
}