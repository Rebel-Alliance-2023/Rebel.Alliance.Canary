using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ValidateRevocationMessage : IActorMessage
    {
        public string CredentialId { get; }

        public ValidateRevocationMessage(string credentialId)
        {
            CredentialId = credentialId ?? throw new System.ArgumentNullException(nameof(credentialId));
        }

        public string MessageType => nameof(ValidateRevocationMessage);
    }
}