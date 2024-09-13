using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class StoreCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(StoreCredentialMessage);
    }
}