using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    // Message classes
    public class CreateCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(CreateCredentialMessage);
    }
}