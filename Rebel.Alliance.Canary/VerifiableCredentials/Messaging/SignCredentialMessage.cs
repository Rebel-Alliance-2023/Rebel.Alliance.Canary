using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class SignCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(SignCredentialMessage);
    }
}