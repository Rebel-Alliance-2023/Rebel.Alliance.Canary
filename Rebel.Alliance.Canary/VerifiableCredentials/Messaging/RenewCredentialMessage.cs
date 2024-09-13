using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RenewCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(RenewCredentialMessage);
    }
}