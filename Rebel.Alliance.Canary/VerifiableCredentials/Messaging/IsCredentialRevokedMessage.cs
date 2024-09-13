using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class IsCredentialRevokedMessage : IActorMessage
    {
        public string CredentialId { get; }

        public IsCredentialRevokedMessage(string credentialId)
        {
            CredentialId = credentialId ?? throw new System.ArgumentNullException(nameof(credentialId));
        }

        public string MessageType => nameof(IsCredentialRevokedMessage);
    }




}
