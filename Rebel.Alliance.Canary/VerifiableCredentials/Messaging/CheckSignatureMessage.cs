using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class CheckSignatureMessage : IActorMessage
    {
        public string MessageType => nameof(CheckSignatureMessage);
    }
}