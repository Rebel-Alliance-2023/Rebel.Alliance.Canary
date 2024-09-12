using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Models;

namespace Rebel.Alliance.Canary.Actors
{
    public partial class CredentialIssuerActor : ActorBase, ICredentialIssuerActor
    {
        private readonly IMediator _mediator;
        private readonly IActorStateManager _stateManager;
        private readonly ITrustFrameworkManagerActor _trustFrameworkManager;

        public CredentialIssuerActor(string id, IMediator mediator, IActorStateManager stateManager, ITrustFrameworkManagerActor trustFrameworkManager)
            : base(id)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _trustFrameworkManager = trustFrameworkManager ?? throw new ArgumentNullException(nameof(trustFrameworkManager));
        }

        public override async Task OnActivateAsync()
        {
            try
            {
                Console.WriteLine($"Activating CredentialIssuerActor with ID: {Id}");
                await base.OnActivateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error activating CredentialIssuerActor: {ex.Message}");
                throw;
            }
        }

        public async Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subject, Dictionary<string, string> claims)
        {
            try
            {
                if (!await ValidateIssuerAsync(issuerId))
                {
                    throw new InvalidOperationException("Issuer is not trusted");
                }

                var credential = new VerifiableCredential
                {
                    Id = Guid.NewGuid().ToString(),
                    Issuer = issuerId,
                    Subject = subject,
                    IssuanceDate = DateTime.UtcNow,
                    Claims = claims
                };

                credential.Proof = await SignCredentialAsync(credential);

                Console.WriteLine($"Credential issued: {credential.Id}");
                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error issuing credential: {ex.Message}");
                throw;
            }
        }

        private async Task<Proof> SignCredentialAsync(VerifiableCredential credential)
        {
            try
            {
                // Use CryptoService to sign the credential (simulated for now)
                var signature = "signed_credential_data"; // Replace with actual signing logic
                return new Proof
                {
                    Created = DateTime.UtcNow,
                    VerificationMethod = "IssuerPublicKey", // Simulated value
                    Jws = signature
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error signing credential: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> ValidateIssuerAsync(string issuerId)
        {
            try
            {
                // Use the TrustFrameworkManagerActor to check if the issuer is trusted
                return await _trustFrameworkManager.IsTrustedIssuerAsync(issuerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating issuer: {ex.Message}");
                throw;
            }
        }
    }

    public interface ICredentialIssuerActor : IActor
    {
        Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subject, Dictionary<string, string> claims);
    }
}
