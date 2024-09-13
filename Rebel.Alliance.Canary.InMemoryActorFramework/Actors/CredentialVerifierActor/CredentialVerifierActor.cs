
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.VerifiableCredentials;


namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor
{
    public class CredentialVerifierActor : ActorBase, ICredentialVerifierActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly IRevocationManagerActor _revocationManagerActor;
        private readonly ILogger<CredentialVerifierActor> _logger;
        private readonly VerifiableCredential _webAppCredential;

        public CredentialVerifierActor(
            string id,
            ICryptoService cryptoService,
            IRevocationManagerActor revocationManagerActor,
            ILogger<CredentialVerifierActor> logger,
            VerifiableCredential webAppCredential) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _revocationManagerActor = revocationManagerActor ?? throw new ArgumentNullException(nameof(revocationManagerActor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webAppCredential = webAppCredential ?? throw new ArgumentNullException(nameof(webAppCredential));
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case VerifyCredentialMessage verifyMsg:
                    return await VerifyCredentialAsync(verifyMsg.Credential);
                case ValidateTokenMessage validateTokenMsg:
                    return await ValidateTokenAsync(validateTokenMsg.Token);
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        public async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
        {
            try
            {
                if (credential == null || !credential.IsValid())
                {
                    _logger.LogWarning($"Invalid credential format for credential {credential?.Id}");
                    return false;
                }

                if (credential.IsExpired)
                {
                    _logger.LogWarning($"Expired credential: {credential.Id}");
                    return false;
                }

                var isSignatureValid = await CheckSignatureAsync(credential);
                if (!isSignatureValid)
                {
                    _logger.LogWarning($"Invalid signature for credential: {credential.Id}");
                    return false;
                }

                var isRevoked = await _revocationManagerActor.IsCredentialRevokedAsync(credential.Id);
                if (isRevoked)
                {
                    _logger.LogWarning($"Revoked credential: {credential.Id}");
                    return false;
                }

                _logger.LogInformation($"Credential verified successfully: {credential.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying credential: {credential?.Id}");
                return false;
            }
        }


        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Received empty or null token for validation");
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_webAppCredential.ClientSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _webAppCredential.Authority,
                    ValidateAudience = true,
                    ValidAudience = _webAppCredential.ClientId,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                    // Additional custom claim validation if needed
                    foreach (var claim in _webAppCredential.Claims)
                    {
                        var tokenClaim = claimsPrincipal.FindFirst(claim.Key);
                        if (tokenClaim == null || tokenClaim.Value != claim.Value)
                        {
                            _logger.LogWarning($"Token is missing or has invalid value for claim: {claim.Key}");
                            return false;
                        }
                    }

                    _logger.LogInformation("Token validated successfully");
                    return true;
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogWarning(ex, "Token validation failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }
        private async Task<bool> CheckSignatureAsync(VerifiableCredential credential)
        {
            try
            {
                var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
                var publicKeyBytes = Convert.FromBase64String(credential.Proof.VerificationMethod);
                var signatureBytes = Convert.FromBase64String(credential.Proof.Jws);

                return await _cryptoService.VerifyDataAsync(publicKeyBytes, credentialData, signatureBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking signature for credential: {credential.Id}");
                return false;
            }
        }
    }
}
