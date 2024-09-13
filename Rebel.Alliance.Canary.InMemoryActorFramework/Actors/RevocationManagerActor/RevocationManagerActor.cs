using System;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.RevocationManagerActor
{

    public class RevocationManagerActor : ActorBase, IRevocationManagerActor
    {
        private readonly IActorStateManager _stateManager;
        private readonly IMediator _mediator;

        public RevocationManagerActor(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task RevokeCredentialAsync(string credentialId)
        {
            // Logic to revoke a credential
            await _stateManager.SetStateAsync(credentialId, true);
        }

        public async Task<bool> IsCredentialRevokedAsync(string credentialId)
        {
            // Logic to check if a credential is revoked
            var isRevoked = await _stateManager.TryGetStateAsync<bool>(credentialId);
            return isRevoked;
        }

        public async Task NotifyRevocationAsync(string credentialId)
        {
            // Logic to notify relevant parties of the revocation
            Console.WriteLine($"Credential {credentialId} has been revoked.");
            await Task.CompletedTask;
        }

        public async Task<bool> ValidateRevocationAsync(string credentialId)
        {
            // Logic to validate if the revocation has occurred
            var isRevoked = await IsCredentialRevokedAsync(credentialId);
            return isRevoked;
        }
    }
}
