using Rebel.Alliance.Canary.Actor.Interfaces;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Messaging
{
    public class ExchangeAuthorizationCodeMessage : IActorMessage
    {
        public string Code { get; }
        public string RedirectUri { get; }
        public string ClientId { get; }

        public ExchangeAuthorizationCodeMessage(string code, string redirectUri, string clientId)
        {
            Code = code;
            RedirectUri = redirectUri;
            ClientId = clientId;
        }

        public string MessageType => nameof(ExchangeAuthorizationCodeMessage);
    }
}