using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class CreateRootCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(CreateRootCredentialMessage);
    }
}