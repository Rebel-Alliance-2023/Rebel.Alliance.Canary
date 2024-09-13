using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class NotifyRevocationMessage : IActorMessage
    {
        public string MessageType => nameof(NotifyRevocationMessage);
    }
}