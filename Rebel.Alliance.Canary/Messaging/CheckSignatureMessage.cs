using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class CheckSignatureMessage : IActorMessage
    {
        public string MessageType => nameof(CheckSignatureMessage);
    }
}