using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.RevocationManagerActor
{
    public class RevocationManagerActor : ActorBase, IRevocationManagerActor
    {
        private readonly IActorStateManager _stateManager;
        private readonly ILogger<RevocationManagerActor> _logger;

        public RevocationManagerActor(
            string id,
            IActorStateManager stateManager,
            ILogger<RevocationManagerActor> logger) : base(id)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case RevokeCredentialMessage revokeMsg:
                    await RevokeCredentialAsync(revokeMsg.CredentialId);
                    return null;
                case IsCredentialRevokedMessage isRevokedMsg:
                    return await IsCredentialRevokedAsync(isRevokedMsg.CredentialId);
                case NotifyRevocationMessage notifyMsg:
                    await NotifyRevocationAsync(notifyMsg.CredentialId);
                    return null;
                case ValidateRevocationMessage validateMsg:
                    return await ValidateRevocationAsync(validateMsg.CredentialId);
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        public async Task RevokeCredentialAsync(string credentialId)
        {
            try
            {
                await _stateManager.SetStateAsync(credentialId, true);
                _logger.LogInformation($"Credential {credentialId} has been revoked.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking credential {credentialId}");
                throw;
            }
        }

        public async Task<bool> IsCredentialRevokedAsync(string credentialId)
        {
            try
            {
                var isRevoked = await _stateManager.TryGetStateAsync<bool>(credentialId);
                return isRevoked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking revocation status for credential {credentialId}");
                throw;
            }
        }

        public async Task NotifyRevocationAsync(string credentialId)
        {
            try
            {
                // Implement notification logic here
                _logger.LogInformation($"Notified revocation for credential {credentialId}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying revocation for credential {credentialId}");
                throw;
            }
        }

        public async Task<bool> ValidateRevocationAsync(string credentialId)
        {
            try
            {
                var isRevoked = await IsCredentialRevokedAsync(credentialId);
                _logger.LogInformation($"Validated revocation for credential {credentialId}. Is revoked: {isRevoked}");
                return isRevoked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating revocation for credential {credentialId}");
                throw;
            }
        }
    }
}
