using System;

namespace Rebel.Alliance.Canary.OIDC.Models
{
    /// <summary>
    /// Represents the response from a token request in the OIDC flow.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// The access token, which is used to access protected resources.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The ID token, which contains claims about the authentication of an end-user.
        /// </summary>
        public string IdToken { get; set; }

        /// <summary>
        /// The expiration time of the tokens.
        /// </summary>
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
