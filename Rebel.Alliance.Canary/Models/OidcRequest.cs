public class OidcRequest
{
    public string ClientId { get; }
    public string RedirectUri { get; }

    public OidcRequest(string clientId, string redirectUri)
    {
        ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
    }
}
