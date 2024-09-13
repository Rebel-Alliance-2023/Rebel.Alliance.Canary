using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class VerifyCredentialChainMessage : IActorMessage
    {
        public string MessageType => nameof(VerifyCredentialChainMessage);
    }
}