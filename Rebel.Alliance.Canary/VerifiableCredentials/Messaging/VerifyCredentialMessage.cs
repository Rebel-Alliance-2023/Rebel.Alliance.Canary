using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class VerifyCredentialMessage : IActorMessage
    {
        public string MessageType => nameof(VerifyCredentialMessage);
    }
}