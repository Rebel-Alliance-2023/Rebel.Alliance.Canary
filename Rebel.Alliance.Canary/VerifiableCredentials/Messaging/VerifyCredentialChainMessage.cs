using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class VerifyCredentialChainMessage : IActorMessage
    {
        public string MessageType => nameof(VerifyCredentialChainMessage);
    }
}