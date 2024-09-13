using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TrustFrameworkManagerActor
{
    public class TrustFrameworkManagerActor : ActorBase, ITrustFrameworkManagerActor
    {
        private readonly ILogger<TrustFrameworkManagerActor> _logger;
        private readonly IActorStateManager _stateManager;
        private readonly HashSet<string> _trustedIssuers = new HashSet<string>();
        private readonly HashSet<string> _revokedIssuers = new HashSet<string>();

        public TrustFrameworkManagerActor(
            string id,
            ILogger<TrustFrameworkManagerActor> logger,
            IActorStateManager stateManager) : base(id)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public override async Task OnActivateAsync()
        {
            _logger.LogInformation($"Activating TrustFrameworkManagerActor with ID: {Id}");
            await base.OnActivateAsync();
            await LoadStateAsync();
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case RegisterIssuerMessage registerMsg:
                    return await RegisterIssuerAsync(registerMsg.IssuerDid, registerMsg.PublicKey);
                case CertifyIssuerMessage certifyMsg:
                    return await CertifyIssuerAsync(certifyMsg.IssuerDid);
                case RevokeIssuerMessage revokeMsg:
                    return await RevokeIssuerAsync(revokeMsg.IssuerDid);
                case IsTrustedIssuerMessage trustedMsg:
                    return await IsTrustedIssuerAsync(trustedMsg.IssuerDid);
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        public async Task<bool> RegisterIssuerAsync(string issuerDid, string publicKey)
        {
            if (_trustedIssuers.Contains(issuerDid) || _revokedIssuers.Contains(issuerDid))
            {
                _logger.LogWarning($"Issuer already registered or revoked: {issuerDid}");
                return false;
            }

            _trustedIssuers.Add(issuerDid);
            await SaveStateAsync();

            _logger.LogInformation($"Issuer registered: {issuerDid}");
            return true;
        }

        public async Task<bool> CertifyIssuerAsync(string issuerDid)
        {
            if (!_trustedIssuers.Contains(issuerDid))
            {
                _logger.LogWarning($"Issuer not found: {issuerDid}");
                return false;
            }

            // Here you might add additional logic for certification if needed

            _logger.LogInformation($"Issuer certified: {issuerDid}");
            return true;
        }

        public async Task<bool> RevokeIssuerAsync(string issuerDid)
        {
            if (!_trustedIssuers.Contains(issuerDid))
            {
                _logger.LogWarning($"Issuer not found: {issuerDid}");
                return false;
            }

            _trustedIssuers.Remove(issuerDid);
            _revokedIssuers.Add(issuerDid);
            await SaveStateAsync();

            _logger.LogInformation($"Issuer revoked: {issuerDid}");
            return true;
        }

        public async Task<bool> IsTrustedIssuerAsync(string issuerDid)
        {
            var isTrusted = _trustedIssuers.Contains(issuerDid);
            _logger.LogInformation($"Issuer trust status checked: {issuerDid}, Is trusted: {isTrusted}");
            return isTrusted;
        }

        private async Task LoadStateAsync()
        {
            var trustedIssuers = await _stateManager.TryGetStateAsync<HashSet<string>>("TrustedIssuers");
            var revokedIssuers = await _stateManager.TryGetStateAsync<HashSet<string>>("RevokedIssuers");

            if (trustedIssuers != null)
                _trustedIssuers.UnionWith(trustedIssuers);

            if (revokedIssuers != null)
                _revokedIssuers.UnionWith(revokedIssuers);
        }

        private async Task SaveStateAsync()
        {
            await _stateManager.SetStateAsync("TrustedIssuers", _trustedIssuers);
            await _stateManager.SetStateAsync("RevokedIssuers", _revokedIssuers);
        }
    }
}
