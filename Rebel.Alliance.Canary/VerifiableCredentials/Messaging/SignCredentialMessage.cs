using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class SignCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(SignCredentialMessage);
    }
}