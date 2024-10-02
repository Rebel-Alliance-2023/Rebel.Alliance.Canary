public class TokenValidationMessage
{
    public string Token { get; }
    public string ExpectedIssuer { get; }
    public string ExpectedAudience { get; }
    public string ClientId { get; }
    public int? ExpirationTime { get; }

    public TokenValidationMessage(string token, string expectedIssuer, string expectedAudience, string clientId, int? expirationTime = null)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpectedIssuer = expectedIssuer ?? throw new ArgumentNullException(nameof(expectedIssuer));
        ExpectedAudience = expectedAudience ?? throw new ArgumentNullException(nameof(expectedAudience));
        ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        ExpirationTime = expirationTime;
    }
}
