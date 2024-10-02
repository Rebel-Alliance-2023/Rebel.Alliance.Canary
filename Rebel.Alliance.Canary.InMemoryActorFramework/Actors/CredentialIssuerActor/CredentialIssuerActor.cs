using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.Models.Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialIssuerActor
{
    public class CredentialIssuerActor : ActorBase, ICredentialIssuerActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<CredentialIssuerActor> _logger;
        private readonly ITrustFrameworkManagerActor _trustFrameworkManager;

        public CredentialIssuerActor(
            string id,
            ICryptoService cryptoService,
            ILogger<CredentialIssuerActor> logger,
            ITrustFrameworkManagerActor trustFrameworkManager) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trustFrameworkManager = trustFrameworkManager ?? throw new ArgumentNullException(nameof(trustFrameworkManager));
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            return message switch
            {
                IssueCredentialMessage issueMsg => await IssueCredentialAsync(issueMsg.IssuerId, issueMsg.SubjectId, issueMsg.Claims),
                _ => throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.")
            };
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
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    Claims = claims
                };

                await SignCredentialAsync(credential);

                _logger.LogInformation($"Credential issued: {credential.Id}");
                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error issuing credential for issuer {issuerId}");
                throw;
            }
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
                _logger.LogError(ex, $"Error validating issuer: {issuerId}");
                throw;
            }
        }
    }
}
