using System;

namespace Rebel.Alliance.Canary.OIDC.Models
{
    /// <summary>
    /// Represents the response from a token request in the OIDC flow.
    /// </summary>
    public class TokenResponse
    {
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        

        public DateTime Expiration { get; set; }

        /// <summary>
        /// Creates a new instance of TokenResponse.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="idToken">The ID token.</param>
        /// <param name="expiration">The expiration time of the tokens.</param>
        public TokenResponse(string accessToken, string idToken, DateTime expiration)
        {
            AccessToken = accessToken;
            IdToken = idToken;
            Expiration = expiration;
        }
    }
}
