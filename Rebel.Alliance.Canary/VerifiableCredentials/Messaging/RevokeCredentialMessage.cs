using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RevokeCredentialMessage : IActorMessage
    {
        public string CredentialId { get; }

        public RevokeCredentialMessage(string credentialId)
        {
            CredentialId = credentialId;
        }

        public string MessageType => nameof(RevokeCredentialMessage);
    }
}