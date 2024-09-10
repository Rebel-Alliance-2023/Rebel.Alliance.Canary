using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // For JWT token handling
using System.Security.Claims; // For claims-based identity
using System.Security.Cryptography;
using System.Text; // For Encoding
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // For configuration management
using Microsoft.IdentityModel.Tokens; // For security tokens and key management
using Rebel.Alliance.Canary.Abstractions; // For interface and model definitions

namespace Rebel.Alliance.Canary.Services
{
    public class OidcProviderService
    {
        private readonly ConcurrentDictionary<string, AuthorizationCode> _authorizationCodes = new();
        private readonly ConcurrentDictionary<string, AccessToken> _accessTokens = new();
        private readonly ConcurrentDictionary<string, IdToken> _idTokens = new();
        private readonly ICryptoService _cryptoService;
        private readonly IConfiguration _configuration;

        public OidcProviderService(ICryptoService cryptoService, IConfiguration configuration)
        {
            _cryptoService = cryptoService;
            _configuration = configuration;
        }

        public async Task<string> GenerateAuthorizationCodeAsync(string clientId, string redirectUri, string userId)
        {

            // Generate 32 random bytes
            byte[] randomBytes = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            // Convert the random bytes to a string representation
            //string randomString = Convert.ToBase64String(randomBytes);
            var code = Convert.ToBase64String(randomBytes);
            var authorizationCode = new AuthorizationCode
            {
                Code = code,
                ClientId = clientId,
                RedirectUri = redirectUri,
                Expiration = DateTime.UtcNow.AddMinutes(5),
                UserId = userId
            };
            _authorizationCodes[code] = authorizationCode;
            return code;
        }

        public async Task<OidcResponse> ExchangeAuthorizationCodeAsync(string code, string clientId, string redirectUri)
        {
            // Retrieve and validate the authorization code
            if (!_authorizationCodes.TryGetValue(code, out var authorizationCode) ||
                authorizationCode.ClientId != clientId ||
                authorizationCode.RedirectUri != redirectUri ||
                authorizationCode.Expiration < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Invalid authorization code or code has expired.");
            }

            // Remove the authorization code after use
            _authorizationCodes.TryRemove(code, out _);

            // Generate Access Token
            var accessToken = GenerateAccessToken(clientId, authorizationCode.UserId);

            // Generate ID Token
            var idToken = await GenerateIdToken(authorizationCode.UserId);

            // Create OIDC Response with generated tokens
            var response = new OidcResponse
            {
                AccessToken = accessToken.Token,
                IdToken = idToken.Token,
                TokenType = "Bearer",
                ExpiresIn = (int)(accessToken.Expiration - DateTime.UtcNow).TotalSeconds
            };

            // Store tokens for later validation or use
            _accessTokens[accessToken.Token] = accessToken;
            _idTokens[idToken.Token] = idToken;

            return response;
        }

        private AccessToken GenerateAccessToken(string clientId, string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // Replace with your key management

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim("client_id", clientId)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = new AccessToken
            {
                Token = tokenHandler.WriteToken(token),
                ClientId = clientId,
                UserId = userId,
                Expiration = tokenDescriptor.Expires.Value
            };
            return accessToken;
        }

        private async Task<IdToken> GenerateIdToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // Replace with your key management

            var userClaims = await GetUserClaimsAsync(userId); // Get user claims from VC

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var idToken = new IdToken
            {
                Token = tokenHandler.WriteToken(token),
                UserId = userId,
                Expiration = tokenDescriptor.Expires.Value
            };
            return idToken;
        }

        private async Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId)
        {
            // Implement logic to retrieve user claims from Verifiable Credentials (VC)
            return new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Name, userId),
        // Add any other claims retrieved from the VC
    };
        }


        // Other methods for token management and validation
    }

    public class AuthorizationCode
    {
        public string Code { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public DateTime Expiration { get; set; }
        public string UserId { get; set; }
    }

    public class AccessToken
    {
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string UserId { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class IdToken
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class OidcResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
}
