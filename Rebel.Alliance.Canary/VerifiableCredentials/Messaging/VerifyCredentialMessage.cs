using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class VerifyCredentialMessage : IActorMessage
    {
        public VerifiableCredential Credential { get; }

        public VerifyCredentialMessage(VerifiableCredential credential)
        {
            Credential = credential ?? throw new System.ArgumentNullException(nameof(credential));
        }

        public string MessageType => nameof(VerifyCredentialMessage);
    }
}
