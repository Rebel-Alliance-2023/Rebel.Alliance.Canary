using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ValidateTokenMessage : IActorMessage
    {
        public string Token { get; }

        public ValidateTokenMessage(string token)
        {
            Token = token ?? throw new System.ArgumentNullException(nameof(token));
        }

        public string MessageType => nameof(ValidateTokenMessage);
    }
}
