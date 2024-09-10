using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Services;

namespace Rebel.Alliance.Canary.Actors
{
    public interface ICredentialHolderActor : IActor
    {
        /// <summary>
        /// Stores a credential securely for the holder.
        /// </summary>
        /// <param name="credential">The verifiable credential to store.</param>
        Task StoreCredentialAsync(VerifiableCredential credential);

        /// <summary>
        /// Presents a stored credential if it exists and is valid.
        /// </summary>
        /// <param name="credentialId">The ID of the credential to present.</param>
        /// <returns>The verifiable credential.</returns>
        Task<VerifiableCredential> PresentCredentialAsync(string credentialId);

        /// <summary>
        /// Renews a credential if it is nearing expiration.
        /// </summary>
        /// <param name="credentialId">The ID of the credential to renew.</param>
        Task RenewCredentialAsync(string credentialId);
    }

    public class CredentialHolderActor : ActorBase, ICredentialHolderActor
    {
        private readonly IMediator _mediator;
        private readonly IActorStateManager _stateManager;
        private readonly ICryptoService _cryptoService;

        public CredentialHolderActor(string id, IMediator mediator, IActorStateManager stateManager, ICryptoService cryptoService)
            : base(id)
        {
            _mediator = mediator;
            _stateManager = stateManager;
            _cryptoService = cryptoService;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            Console.WriteLine($"CredentialHolderActor {Id} activated.");
        }

        public async Task StoreCredentialAsync(VerifiableCredential credential)
        {
            // Store the credential in state
            await _stateManager.SetStateAsync($"Credential:{credential.Id}", credential);
            Console.WriteLine($"Stored credential {credential.Id} for actor {Id}.");
        }

        public async Task<VerifiableCredential> PresentCredentialAsync(string credentialId)
        {
            // Retrieve the credential from state
            var credential = await _stateManager.TryGetStateAsync<VerifiableCredential>($"Credential:{credentialId}");

            if (credential == null)
            {
                throw new InvalidOperationException($"Credential {credentialId} not found for actor {Id}.");
            }

            // Check if credential is expired
            if (credential.ExpirationDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException($"Credential {credentialId} has expired.");
            }

            Console.WriteLine($"Presenting credential {credentialId} for actor {Id}.");
            return credential;
        }

        public async Task RenewCredentialAsync(string credentialId)
        {
            var credential = await _stateManager.TryGetStateAsync<VerifiableCredential>($"Credential:{credentialId}");

            if (credential == null)
            {
                throw new InvalidOperationException($"Credential {credentialId} not found for actor {Id}.");
            }

            // Check if credential needs renewal (e.g., near expiration)
            if (credential.ExpirationDate <= DateTime.UtcNow.AddDays(7)) // Renew if less than 7 days remaining
            {
                credential.ExpirationDate = DateTime.UtcNow.AddYears(1); // Example: Extend for another year
                await _stateManager.SetStateAsync($"Credential:{credential.Id}", credential);
                Console.WriteLine($"Credential {credentialId} renewed for actor {Id}.");
            }
        }
    }
}

