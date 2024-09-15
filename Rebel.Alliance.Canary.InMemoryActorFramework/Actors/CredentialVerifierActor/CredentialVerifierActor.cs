
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
using System.Security.Cryptography;


namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor
{

public class CredentialVerifierActor : ActorBase, ICredentialVerifierActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly IRevocationManagerActor _revocationManagerActor;
        private readonly ILogger<CredentialVerifierActor> _logger;
        private readonly VerifiableCredential _webAppCredential;
        private readonly IKeyStore _keyStore;

        public CredentialVerifierActor(
            string id,
            ICryptoService cryptoService,
            IRevocationManagerActor revocationManagerActor,
            ILogger<CredentialVerifierActor> logger,
            VerifiableCredential webAppCredential,
            IKeyStore keyStore) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _revocationManagerActor = revocationManagerActor ?? throw new ArgumentNullException(nameof(revocationManagerActor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webAppCredential = webAppCredential ?? throw new ArgumentNullException(nameof(webAppCredential));
            _keyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));

            // Add this check
            if (string.IsNullOrEmpty(_webAppCredential.ClientSecret) ||
                string.IsNullOrEmpty(_webAppCredential.Authority) ||
                string.IsNullOrEmpty(_webAppCredential.ClientId))
            {
                throw new ArgumentException("WebAppCredential is not properly initialized");
            }
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

                _logger.LogInformation($"Validating token: {token}");

                var tokenHandler = new JwtSecurityTokenHandler();
                byte[] publicKeyBytes = await _keyStore.RetrievePublicKeyAsync(_webAppCredential.ClientId);

                _logger.LogInformation($"Using client ID: {_webAppCredential.ClientId}");
                _logger.LogInformation($"Authority: {_webAppCredential.Authority}");

                // Convert byte array to RSA instance
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuer = true,
                    ValidIssuer = _webAppCredential.ClientId,
                    ValidateAudience = true,
                    ValidAudience = _webAppCredential.ClientId,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                    _logger.LogInformation("Token validated. Checking custom claims...");

                    foreach (var claim in claimsPrincipal.Claims)
                    {
                        _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
                    }

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
