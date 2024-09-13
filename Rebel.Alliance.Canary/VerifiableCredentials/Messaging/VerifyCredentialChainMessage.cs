using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class VerifyCredentialChainMessage : IActorMessage
    {
        public VerifiableCredential Credential { get; }
        public VerifiableCredential RootCredential { get; }

        public VerifyCredentialChainMessage(VerifiableCredential credential, VerifiableCredential rootCredential)
        {
            Credential = credential;
            RootCredential = rootCredential;
        }

        public string MessageType => nameof(VerifyCredentialChainMessage);
    }
}