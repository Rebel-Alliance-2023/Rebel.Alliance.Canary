using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialHolderActor
{

    public class CredentialHolderActor : ActorBase, ICredentialHolderActor
    {
        private readonly IMediator _mediator;
        private readonly IActorStateManager _stateManager;
        private readonly ICryptoService _cryptoService;

        public CredentialHolderActor(string id, IMediator mediator, IActorStateManager stateManager, ICryptoService cryptoService)
            : base(id)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                await base.OnActivateAsync();
                Console.WriteLine($"CredentialHolderActor {Id} activated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error activating CredentialHolderActor: {ex.Message}");
                throw;
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

                await _stateManager.SetStateAsync($"Credential:{credential.Id}", credential);
                Console.WriteLine($"Stored credential {credential.Id} for actor {Id}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing credential: {ex.Message}");
                throw;
            }
        }

        public async Task<VerifiableCredential> PresentCredentialAsync(string credentialId)
        {
            try
            {
                var credential = await _stateManager.TryGetStateAsync<VerifiableCredential>($"Credential:{credentialId}");

                if (credential == null)
                {
                    throw new InvalidOperationException($"Credential {credentialId} not found for actor {Id}.");
                }

                if (credential.ExpirationDate <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException($"Credential {credentialId} has expired.");
                }

                Console.WriteLine($"Presenting credential {credentialId} for actor {Id}.");
                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error presenting credential: {ex.Message}");
                throw;
            }
        }

        public async Task RenewCredentialAsync(string credentialId)
        {
            try
            {
                var credential = await _stateManager.TryGetStateAsync<VerifiableCredential>($"Credential:{credentialId}");

                if (credential == null)
                {
                    throw new InvalidOperationException($"Credential {credentialId} not found for actor {Id}.");
                }

                if (credential.ExpirationDate <= DateTime.UtcNow.AddDays(7))
                {
                    credential.ExpirationDate = DateTime.UtcNow.AddYears(1);
                    await _stateManager.SetStateAsync($"Credential:{credential.Id}", credential);
                    Console.WriteLine($"Credential {credentialId} renewed for actor {Id}.");
                }
                else
                {
                    Console.WriteLine($"Credential {credentialId} does not need renewal yet.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renewing credential: {ex.Message}");
                throw;
            }
        }
    }
}
