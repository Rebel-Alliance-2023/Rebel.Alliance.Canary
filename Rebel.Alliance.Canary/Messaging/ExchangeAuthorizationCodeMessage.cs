using Rebel.Alliance.Canary.Abstractions;

namespace Rebel.Alliance.Canary.Messaging
{
    public class ExchangeAuthorizationCodeMessage : IActorMessage
    {
        public string Code { get; private set; }
        public string ClientId { get; private set; }

        public ExchangeAuthorizationCodeMessage(string code, string clientId)
        {
            Code = code;
            ClientId = clientId;
        }

        public string MessageType => nameof(ExchangeAuthorizationCodeMessage);

    }
}