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

namespace Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialAsRootOfTrustActor
{
    public class VerifiableCredentialAsRootOfTrustActor : ActorBase, IVerifiableCredentialAsRootOfTrustActor
    {
        private readonly ICryptoService _cryptoService;
        private readonly IKeyManagementService _keyManagementService;
        private readonly ILogger<VerifiableCredentialAsRootOfTrustActor> _logger;
        private readonly IActorStateManager _stateManager;

        public IActorMessageBus ActorMessageBus { get; }
        public ILogger<VerifiableCredentialAsRootOfTrustActor> Logger { get; }

        public VerifiableCredentialAsRootOfTrustActor(
            string id,
            ICryptoService cryptoService,
            IKeyManagementService keyManagementService,
            ILogger<VerifiableCredentialAsRootOfTrustActor> logger,
            IActorStateManager stateManager) : base(id)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public VerifiableCredentialAsRootOfTrustActor(string? id, IActorMessageBus actorMessageBus, ILogger<VerifiableCredentialAsRootOfTrustActor> logger) : base(id)
        {
            ActorMessageBus = actorMessageBus;
            Logger = logger;
        }

        public override async Task<object> ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case CreateRootCredentialMessage createRootMsg:
                    return await CreateRootCredentialAsync(createRootMsg.IssuerId, createRootMsg.Claims, createRootMsg.MasterKeyId);
                case IssueSubordinateCredentialMessage issueSubMsg:
                    return await IssueSubordinateCredentialAsync(issueSubMsg.IssuerId, issueSubMsg.RootCredential, issueSubMsg.Claims, issueSubMsg.DerivedKeyId);
                case VerifyCredentialChainMessage verifyChainMsg:
                    return await VerifyCredentialChainAsync(verifyChainMsg.Credential, verifyChainMsg.RootCredential);
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        public async Task<VerifiableCredential> CreateRootCredentialAsync(string issuerId, Dictionary<string, string> claims, string masterKeyId)
        {
            try
            {
                var masterKey = await _keyManagementService.GetMasterKeyAsync(masterKeyId);
                if (masterKey == null)
                {
                    throw new InvalidOperationException("Master key not found");
                }

                var credential = new VerifiableCredential
                {
                    Id = Guid.NewGuid().ToString(),
                    Issuer = issuerId,
                    IssuanceDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    Claims = claims
                };

                var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
                var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(masterKeyId, credentialData);

                credential.Proof = new Proof
                {
                    Type = "Ed25519Signature2018",
                    Created = DateTime.UtcNow,
                    VerificationMethod = Convert.ToBase64String(publicKey),
                    ProofPurpose = "assertionMethod",
                    Jws = Convert.ToBase64String(signature)
                };

                await _stateManager.SetStateAsync("RootCredential", credential);

                _logger.LogInformation($"Root credential created: {credential.Id}");
                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating root credential");
                throw;
            }
        }

        public async Task<VerifiableCredential> IssueSubordinateCredentialAsync(string issuerId, VerifiableCredential rootCredential, Dictionary<string, string> claims, string derivedKeyId)
        {
            try
            {
                if (!await VerifyCredentialChainAsync(rootCredential, rootCredential))
                {
                    throw new InvalidOperationException("Root credential is not valid");
                }

                var derivedKey = await _keyManagementService.GetDerivedKeyAsync(derivedKeyId);
                if (derivedKey == null)
                {
                    throw new InvalidOperationException("Derived key not found");
                }

                claims["ParentCredentialId"] = rootCredential.Id;
                var credential = new VerifiableCredential
                {
                    Id = Guid.NewGuid().ToString(),
                    Issuer = issuerId,
                    IssuanceDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    Claims = claims,
                    ParentCredentialId = rootCredential.Id
                };

                var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
                var (signature, publicKey) = await _cryptoService.SignDataUsingIdentifierAsync(derivedKeyId, credentialData);

                credential.Proof = new Proof
                {
                    Type = "Ed25519Signature2018",
                    Created = DateTime.UtcNow,
                    VerificationMethod = Convert.ToBase64String(publicKey),
                    ProofPurpose = "assertionMethod",
                    Jws = Convert.ToBase64String(signature)
                };

                _logger.LogInformation($"Subordinate credential issued: {credential.Id}");
                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing subordinate credential");
                throw;
            }
        }

        public async Task<bool> VerifyCredentialChainAsync(VerifiableCredential credential, VerifiableCredential rootCredential)
        {
            try
            {
                if (!await VerifyCredentialAsync(rootCredential))
                {
                    return false;
                }

                var currentCredential = credential;
                while (!string.IsNullOrEmpty(currentCredential.ParentCredentialId))
                {
                    if (!await VerifyCredentialAsync(currentCredential))
                    {
                        return false;
                    }

                    currentCredential = await _stateManager.GetStateAsync<VerifiableCredential>(currentCredential.ParentCredentialId);
                }

                var isValid = currentCredential.Id == rootCredential.Id;
                _logger.LogInformation($"Credential chain verification result: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying credential chain");
                return false;
            }
        }

        private async Task<bool> VerifyCredentialAsync(VerifiableCredential credential)
        {
            try
            {
                var keyId = credential.Proof.VerificationMethod;
                byte[] publicKey = null;

                var masterKey = await _keyManagementService.GetMasterKeyAsync(keyId);
                if (masterKey != null)
                {
                    publicKey = masterKey.PublicKey;
                }
                else
                {
                    var derivedKey = await _keyManagementService.GetDerivedKeyAsync(keyId);
                    if (derivedKey != null)
                    {
                        publicKey = derivedKey.PublicKey;
                    }
                }

                if (publicKey == null)
                {
                    return false;
                }

                var credentialData = $"{credential.Issuer}|{credential.IssuanceDate}|{string.Join(",", credential.Claims)}";
                var signature = Convert.FromBase64String(credential.Proof.Jws);

                return await _cryptoService.VerifyDataAsync(publicKey, credentialData, signature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying credential: {credential.Id}");
                return false;
            }
        }
    }
}
