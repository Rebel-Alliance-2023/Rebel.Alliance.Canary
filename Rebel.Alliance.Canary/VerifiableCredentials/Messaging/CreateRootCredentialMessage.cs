using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class CreateRootCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(CreateRootCredentialMessage);
    }
}