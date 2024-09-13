using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class RegisterIssuerMessage : IActorMessage
    {
        public string MessageType => nameof(RegisterIssuerMessage);
    }
}