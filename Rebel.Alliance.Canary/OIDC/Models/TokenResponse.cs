namespace Rebel.Alliance.Canary.OIDC.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }

        public TokenResponse(string token, DateTime expiration)
        {
            Token = token;
            Expiration = expiration;
        }
    }
}
