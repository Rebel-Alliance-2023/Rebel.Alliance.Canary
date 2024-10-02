using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialHolderActor
{
    public class CredentialHolderActor : ActorBase, ICredentialHolderActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<CredentialHolderActor> _logger;

        public CredentialHolderActor(
            string id,
            ICryptoService cryptoService,
            ILogger<CredentialHolderActor> logger) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case StoreCredentialMessage storeMsg:
                    await StoreCredentialAsync(storeMsg.VerifiableCredential);
                    return null; // or return a confirmation message if needed

                case PresentCredentialMessage presentMsg:
                    return await PresentCredentialAsync(presentMsg.CredentialId);

                case RenewCredentialMessage renewMsg:
                    await RenewCredentialAsync(renewMsg.CredentialId);
                    return null; // or return a confirmation message if needed

                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        public async Task StoreCredentialAsync(VerifiableCredential credential)
        {
            try
            {
                if (credential == null)
                {
                    throw new ArgumentNullException(nameof(credential));
                }

                string key = $"Credential:{credential.Id}";
                await StateManager.SetStateAsync(key, credential);
                _logger.LogInformation($"Stored credential {credential.Id} for actor {Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error storing credential for actor {Id}");
                throw;
            }
        }

        public async Task<VerifiableCredential> PresentCredentialAsync(string credentialId)
        {
            try
            {
                string key = $"Credential:{credentialId}";
                var credential = await StateManager.TryGetStateAsync<VerifiableCredential>(key);

                if (credential == null)
                {
                    _logger.LogWarning($"Credential {credentialId} not found for actor {Id}");
                    throw new InvalidOperationException($"Credential {credentialId} not found");
                }

                if (credential.IsExpired)
                {
                    _logger.LogWarning($"Credential {credentialId} has expired for actor {Id}");
                    throw new InvalidOperationException($"Credential {credentialId} has expired");
                }

                _logger.LogInformation($"Presenting credential {credentialId} for actor {Id}");
                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error presenting credential {credentialId} for actor {Id}");
                throw;
            }
        }

        public async Task RenewCredentialAsync(string credentialId)
        {
            try
            {
                string key = $"Credential:{credentialId}";
                var credential = await StateManager.TryGetStateAsync<VerifiableCredential>(key);

                if (credential == null)
                {
                    _logger.LogWarning($"Credential {credentialId} not found for actor {Id}");
                    throw new InvalidOperationException($"Credential {credentialId} not found");
                }

                // Check if the credential is close to expiration (e.g., within 30 days)
                if (credential.ExpirationDate <= DateTime.UtcNow.AddDays(30))
                {
                    // Extend the expiration date by one year from now
                    credential.ExpirationDate = DateTime.UtcNow.AddYears(1);

                    // Re-sign the credential with the new expiration date
                    var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{credential.ExpirationDate}|{string.Join(",", credential.Claims)}";
                    var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(credential.Issuer, credentialData);

                    credential.Proof.Created = DateTime.UtcNow;
                    credential.Proof.VerificationMethod = Convert.ToBase64String(publicKey);
                    credential.Proof.Jws = Convert.ToBase64String(signature);

                    await StateManager.SetStateAsync(key, credential);
                    _logger.LogInformation($"Renewed credential {credentialId} for actor {Id}");
                }
                else
                {
                    _logger.LogInformation($"Credential {credentialId} for actor {Id} does not need renewal yet");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error renewing credential {credentialId} for actor {Id}");
                throw;
            }
        }
    }
}
