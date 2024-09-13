using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.Models.Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialIssuerActor
{

    public class CredentialIssuerActor : ActorBase, ICredentialIssuerActor
    {
        private readonly IMediator _mediator;
        private readonly IActorStateManager _stateManager;
        private readonly ITrustFrameworkManagerActor _trustFrameworkManager;
        private readonly ICryptoService _cryptoService;

        public CredentialIssuerActor(string id, IMediator mediator, IActorStateManager stateManager, ITrustFrameworkManagerActor trustFrameworkManager, ICryptoService cryptoService)
            : base(id)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _trustFrameworkManager = trustFrameworkManager ?? throw new ArgumentNullException(nameof(trustFrameworkManager));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
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
            if (!await ValidateIssuerAsync(issuerId))
            {
                throw new InvalidOperationException("Issuer is not trusted");
            }

            var credential = new VerifiableCredential
            {
                Issuer = issuerId,
                Subject = subject,
                IssuanceDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1), // Set a default expiration
                Claims = claims
            };

            await SignCredentialAsync(credential);

            Console.WriteLine($"Credential issued: {credential.Id}");
            return credential;
        }

        private async Task SignCredentialAsync(VerifiableCredential credential)
        {
            var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
            var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(credential.Issuer, credentialData);

            credential.Proof = new Proof
            {
                Type = "Ed25519Signature2018",
                Created = DateTime.UtcNow,
                VerificationMethod = Convert.ToBase64String(publicKey),
                ProofPurpose = "assertionMethod",
                Jws = Convert.ToBase64String(signature)
            };
        }

        private async Task<bool> ValidateIssuerAsync(string issuerId)
        {
            try
            {
                return await _trustFrameworkManager.IsTrustedIssuerAsync(issuerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating issuer: {ex.Message}");
                throw;
            }
        }
    }
}