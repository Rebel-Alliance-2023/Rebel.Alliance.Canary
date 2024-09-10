using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class InitiateAuthenticationMessage : IActorMessage
    {
        public string MessageType => nameof(InitiateAuthenticationMessage);

        // New properties to match the arguments expected
        public string ClientId { get; }
        public string RedirectUri { get; }

        // Constructor to accept required arguments
        public InitiateAuthenticationMessage(string clientId, string redirectUri)
        {
            ClientId = clientId;
            RedirectUri = redirectUri;
        }
    }
}