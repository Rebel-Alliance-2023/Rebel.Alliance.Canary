using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class RegisterIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(RegisterIssuerMessage);
    }
}