using Rebel.Alliance.Canary.Models;

public class TokenRequestMessage
{
    public VerifiableCredential ClientCredential { get; }
    public string RedirectUri { get; }
    public string ClientId { get; }
    public string ReplyTo { get; }  // Add ReplyTo property

    public TokenRequestMessage(VerifiableCredential clientCredential, string redirectUri, string clientId, string replyTo)
    {
        ClientCredential = clientCredential ?? throw new ArgumentNullException(nameof(clientCredential));
        RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
        ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        ReplyTo = replyTo ?? throw new ArgumentNullException(nameof(replyTo));  // Initialize ReplyTo
    }
}
