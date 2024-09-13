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

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialActor
{
    public class VerifiableCredentialActor : ActorBase, IVerifiableCredentialActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<VerifiableCredentialActor> _logger;
        private readonly IActorStateManager _stateManager;

        public VerifiableCredentialActor(
            string id,
            ICryptoService cryptoService,
            ILogger<VerifiableCredentialActor> logger,
            IActorStateManager stateManager) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case SignCredentialMessage signMsg:
                    return await SignCredentialAsync(signMsg.Credential);
                case IssueCredentialMessage issueMsg:
                    return await IssueCredentialAsync(issueMsg.IssuerId, issueMsg.SubjectId, issueMsg.ExpirationDate, issueMsg.Claims);
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        public async Task<bool> SignCredentialAsync(VerifiableCredential credential)
        {
            try
            {
                if (credential == null)
                {
                    throw new ArgumentNullException(nameof(credential));
                }

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

                _logger.LogInformation($"Credential signed successfully: {credential.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error signing credential: {credential?.Id}");
                return false;
            }
        }

        public async Task<VerifiableCredential> IssueCredentialAsync(string issuerId, string subjectId, DateTime expirationDate, Dictionary<string, string> claims)
        {
            try
            {
                var credential = new VerifiableCredential
                {
                    Id = Guid.NewGuid().ToString(),
                    Issuer = issuerId,
                    Subject = subjectId,
                    IssuanceDate = DateTime.UtcNow,
                    ExpirationDate = expirationDate,
                    Claims = claims
                };

                var signed = await SignCredentialAsync(credential);
                if (!signed)
                {
                    throw new InvalidOperationException("Credential could not be signed.");
                }

                await _stateManager.SetStateAsync($"Credential:{credential.Id}", credential);

                _logger.LogInformation($"Credential issued successfully: {credential.Id}");
                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error issuing credential for issuer {issuerId}");
                throw;
            }
        }
    }
}
