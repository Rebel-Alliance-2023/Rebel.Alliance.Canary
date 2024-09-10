using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class VerifyCredentialChainMessage : IActorMessage
    {
        public string MessageType => nameof(VerifyCredentialChainMessage);
    }
}