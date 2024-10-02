
using Rebel.Alliance.Canary.Models;
public class TokenRequest
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string GrantType { get; set; }
    public string Code { get; set; }
    public string RedirectUri { get; set; }
}