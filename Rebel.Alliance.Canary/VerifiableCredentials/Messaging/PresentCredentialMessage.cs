using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class PresentCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(PresentCredentialMessage);
    }
}