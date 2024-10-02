using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class UpdateRegistryMessage : IActorMessage
    {
        public string MessageType => nameof(UpdateRegistryMessage);
    }
}