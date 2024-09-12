using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Abstractions.Actors;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TrustFrameworkManagerActor
{
    public class TrustFrameworkManagerActor : ActorBase, ITrustFrameworkManagerActor
    {
        private readonly IMediator _mediator;
        private readonly IActorStateManager _stateManager;
        private readonly HashSet<string> _trustedIssuers = new HashSet<string>();
        private readonly HashSet<string> _revokedIssuers = new HashSet<string>();

        public TrustFrameworkManagerActor(string id, IMediator mediator, IActorStateManager stateManager)
            : base(id)
        {
            _mediator = mediator;
            _stateManager = stateManager;
        }

        public override async Task OnActivateAsync()
        {
            Console.WriteLine($"Activating TrustFrameworkManagerActor with ID: {Id}");
            await base.OnActivateAsync();
        }

        public async Task<bool> RegisterIssuerAsync(string issuerDid, string publicKey)
        {
            if (_trustedIssuers.Contains(issuerDid) || _revokedIssuers.Contains(issuerDid))
            {
                Console.WriteLine($"Issuer already registered or revoked: {issuerDid}");
                return false;
            }

            _trustedIssuers.Add(issuerDid);
            await _stateManager.SetStateAsync("TrustedIssuers", _trustedIssuers);

            Console.WriteLine($"Issuer registered: {issuerDid}");
            return true;
        }

        public async Task<bool> CertifyIssuerAsync(string issuerDid)
        {
            if (!_trustedIssuers.Contains(issuerDid))
            {
                Console.WriteLine($"Issuer not found: {issuerDid}");
                return false;
            }

            // Mark the issuer as certified (could involve additional state management)
            Console.WriteLine($"Issuer certified: {issuerDid}");
            return true;
        }

        public async Task<bool> RevokeIssuerAsync(string issuerDid)
        {
            if (!_trustedIssuers.Contains(issuerDid))
            {
                Console.WriteLine($"Issuer not found: {issuerDid}");
                return false;
            }

            _trustedIssuers.Remove(issuerDid);
            _revokedIssuers.Add(issuerDid);
            await _stateManager.SetStateAsync("TrustedIssuers", _trustedIssuers);
            await _stateManager.SetStateAsync("RevokedIssuers", _revokedIssuers);

            Console.WriteLine($"Issuer revoked: {issuerDid}");
            return true;
        }

        public async Task<bool> IsTrustedIssuerAsync(string issuerDid)
        {
            if (_trustedIssuers.Contains(issuerDid))
            {
                Console.WriteLine($"Issuer is trusted: {issuerDid}");
                return true;
            }

            Console.WriteLine($"Issuer is not trusted: {issuerDid}");
            return false;
        }
    }
}
