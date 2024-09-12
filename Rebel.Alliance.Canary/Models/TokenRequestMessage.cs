using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Models;

public class TokenRequestMessage : IActorMessage
{
    public VerifiableCredential ClientCredential { get; }
    public string RedirectUri { get; }
    public string ClientId { get; }
    public string ReplyTo { get; }

    public TokenRequestMessage(VerifiableCredential clientCredential, string redirectUri, string clientId, string replyTo)
    {
        ClientCredential = clientCredential;
        RedirectUri = redirectUri;
        ClientId = clientId;
        ReplyTo = replyTo;
    }

    public string MessageType => nameof(TokenRequestMessage);
}
