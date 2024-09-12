using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
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