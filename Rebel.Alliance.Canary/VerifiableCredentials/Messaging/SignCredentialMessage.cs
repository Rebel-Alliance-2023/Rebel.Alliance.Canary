using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class SignCredentialMessage : IActorMessage
    {
        public VerifiableCredential Credential { get; }

        public SignCredentialMessage(VerifiableCredential credential)
        {
            Credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        public string MessageType => nameof(SignCredentialMessage);
    }
}