using Rebel.Alliance.Canary.VerifiableCredentials;

public class AuthenticationRequest
{
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public VerifiableCredential VerifiableCredential { get; set; }
}
