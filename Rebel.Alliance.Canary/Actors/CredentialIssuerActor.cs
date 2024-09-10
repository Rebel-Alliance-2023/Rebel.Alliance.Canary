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
            _mediator = mediator;
            _stateManager = stateManager;
            _trustFrameworkManager = trustFrameworkManager;
        }

        public override async Task OnActivateAsync()
        {
            // Initialize actor state or perform setup tasks
            Console.WriteLine($"Activating CredentialIssuerActor with ID: {Id}");
            await base.OnActivateAsync();
        }

        public async Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subject, Dictionary<string, string> claims)
        {
            if (!await ValidateIssuerAsync(issuerId))
            {
                throw new InvalidOperationException("Issuer is not trusted");
            }

            // Create a new credential
            var credential = new VerifiableCredential
            {
                Id = Guid.NewGuid().ToString(),
                Issuer = issuerId,
                IssuanceDate = DateTime.UtcNow,
                Claims = claims
            };

            // Sign the credential
            credential.Proof = await SignCredentialAsync(credential);

            Console.WriteLine($"Credential issued: {credential.Id}");
            return credential;
        }

        private async Task<Proof> SignCredentialAsync(VerifiableCredential credential)
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

        private async Task<bool> ValidateIssuerAsync(string issuerId)
        {
            // Use the TrustFrameworkManagerActor to check if the issuer is trusted
            return await _trustFrameworkManager.IsTrustedIssuerAsync(issuerId);
        }
    }

    public interface ICredentialIssuerActor : IActor
    {
        Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subject, Dictionary<string, string> claims);
    }
}
