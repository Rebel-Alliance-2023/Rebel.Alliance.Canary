using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class StoreCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(StoreCredentialMessage);
    }
}